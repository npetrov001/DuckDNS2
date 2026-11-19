using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace DuckDNS2
{
    /// <summary>
    /// DuckDNS configuration + update logic (ported from the original DuckDNS client).
    /// The configuration file format (DSON) is 100% compatible with the original DuckDNS.cfg.
    /// </summary>
    class DDns
    {
        private WebClient wclient;

        private WebClient Cli()
        {
            if (wclient == null)
                wclient = new WebClient();
            return wclient;
        }

        /// <summary>
        /// Updates the DuckDNS domain(s). Any failure is added to <paramref name="messages"/>.
        /// An empty message list on return means success.
        /// </summary>
        public void Update(List<string> messages)
        {
            try
            {
                if (Domain == "*")
                {
                    if (Domains.Count == 0)
                        messages.Add("No domains configured");
                    foreach (DDnsDomain d in Domains)
                        UpdateDomain(d, messages);
                }
                else
                {
                    string url = BuildURL(Domain, Token, "", "");
                    string ret = Cli().DownloadString(url);
                    if (ret != "OK")
                        messages.Add(Domain + ": Failed (server answered '" + ret + "')");
                }
            }
            catch (Exception e)
            {
                messages.Add(e.GetType().Name + ": " + e.Message);
            }
        }

        private void UpdateDomain(DDnsDomain d, List<string> messages)
        {
            try
            {
                string ipv4;
                string ipv6;
                switch (d.ResolutionMode)
                {
                    case DDnsResolutionMode.Server:
                        ipv4 = "";
                        ipv6 = "";
                        break;
                    case DDnsResolutionMode.Local:
                        getHostIPs(Dns.GetHostName(), out ipv4, out ipv6);
                        break;
                    case DDnsResolutionMode.Fixed:
                        IPset(d.ResolutionValue, out ipv4, out ipv6);
                        break;
                    case DDnsResolutionMode.Host:
                        getHostIPs(d.ResolutionValue, out ipv4, out ipv6);
                        break;
                    case DDnsResolutionMode.WebService:
                        IPset(Cli().DownloadString(d.ResolutionValue).Trim(), out ipv4, out ipv6);
                        break;
                    default:
                        throw new Exception("Resolution mode not implemented");
                }
                if (ipv4 == null && ipv6 == null)
                {
                    messages.Add(d.Domain + ": IP resolution failed");
                    return;
                }
                string url = BuildURL(d.Domain, Token, ipv4, ipv6);
                string ret;
                if (!string.IsNullOrEmpty(d.BindIP))
                {
                    HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(url);
                    rq.ServicePoint.BindIPEndPointDelegate = (sp, remoteEndPoint, retryCount) =>
                    {
                        return new IPEndPoint(IPAddress.Parse(d.BindIP), 0);
                    };
                    rq.KeepAlive = false;
                    using (WebResponse response = rq.GetResponse())
                    using (Stream s = response.GetResponseStream())
                    using (StreamReader sr = new StreamReader(s, Encoding.ASCII))
                        ret = sr.ReadLine();
                }
                else
                {
                    ret = Cli().DownloadString(url);
                }
                if (ret != "OK")
                    messages.Add(d.Domain + ": Failed (server answered '" + ret + "')");
            }
            catch (Exception e)
            {
                messages.Add(d.Domain + ": " + e.GetType().Name + ": " + e.Message);
            }
        }

        private string BuildURL(string domain, string token, string ipv4, string ipv6)
        {
            string url = ServiceURL;
            url = url.Replace("<DOM>", domain ?? "");
            url = url.Replace("<TKN>", token ?? "");
            url = url.Replace("<IP4>", ipv4 ?? "");
            url = url.Replace("<IP6>", ipv6 ?? "");
            return url;
        }

        private void getHostIPs(string host, out string ipv4, out string ipv6)
        {
            ipv4 = null;
            ipv6 = null;
            IPHostEntry heserver = Dns.GetHostEntry(host);
            foreach (IPAddress curAdd in heserver.AddressList)
            {
                if (ipv6 == null && curAdd.AddressFamily == AddressFamily.InterNetworkV6 && (!curAdd.IsIPv6Multicast) && (!curAdd.IsIPv6LinkLocal) && (!curAdd.IsIPv6SiteLocal))
                    ipv6 = curAdd.ToString();
                if (ipv4 == null && curAdd.AddressFamily == AddressFamily.InterNetwork)
                    ipv4 = curAdd.ToString();
                if (ipv4 != null && ipv6 != null)
                    return;
            }
        }

        private void IPset(string ip, out string ipv4, out string ipv6)
        {
            IPAddress ipa = IPAddress.Parse(ip);
            switch (ipa.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    ipv4 = ip;
                    ipv6 = null;
                    break;
                case AddressFamily.InterNetworkV6:
                    ipv4 = null;
                    ipv6 = ip;
                    break;
                default:
                    throw new Exception("'" + ip + "' is not a valid IP address");
            }
        }

        #region Load / Save

        private void LoadV1(string[] data)
        {
            Domains.Clear();
            Domain = data != null && data.Length > 0 ? data[0] : "";
            Token = data != null && data.Length > 1 ? CharSwitch(data[1]) : "";
            Interval = data != null && data.Length > 2 ? data[2] : "30m";
            var ownResolveOfIPs = data != null && data.Length > 3 ? data[3] == "OwnResolveIpv6" || data[3] == "OwnResolveIPs" : false;
            if (ownResolveOfIPs)
            {
                Domains.Add(new DDnsDomain() { Domain = Domain, ResolutionMode = DDnsResolutionMode.Local });
                Domain = "*";
            }
        }

        private void LoadV2(string[] data)
        {
            Domains.Clear();
            DSON.DeserializeInto(this, data);
            Token = CharSwitch(Token ?? "");
        }

        private static int ConfVersion(string[] data)
        {
            if (data.Length > 0 && data[0].Contains(':'))
                return 2;
            return 1;
        }

        /// <summary>Loads the configuration from <see cref="AppPaths.ConfigFile"/>.</summary>
        public bool Load()
        {
            return LoadFrom(AppPaths.ConfigFile);
        }

        /// <summary>Loads configuration from any DuckDNS.cfg file (used for importing the old client config).</summary>
        public bool LoadFrom(string filepath)
        {
            string[] data = null;
            if (File.Exists(filepath))
            {
                try
                {
                    data = File.ReadAllLines(filepath);
                }
                catch (Exception ex)
                {
                    Log.Error("Cannot read config " + filepath + ": " + ex.Message);
                }
            }
            if (data == null)
                return false;
            switch (ConfVersion(data))
            {
                case 1:
                    LoadV1(data);
                    break;
                case 2:
                    LoadV2(data);
                    break;
            }
            return true;
        }

        /// <summary>Saves the configuration to <see cref="AppPaths.ConfigFile"/>. Throws on error.</summary>
        public void Save()
        {
            AppPaths.EnsureDataDir();
            string tk = Token;
            try
            {
                Token = CharSwitch(Token ?? "");
                File.WriteAllLines(AppPaths.ConfigFile, DSON.Serialize(this));
            }
            finally
            {
                Token = tk;
            }
        }

        private static string CharSwitch(string str)
        { // Super basic, but more than nothing (kept for compatibility with the original config files)
            string a = "abcdef0123456789";
            string b = "f9031ace7d86524b";
            StringBuilder sb = new StringBuilder(str);
            for (int i = 0; i < sb.Length; i++)
            {
                int chi = a.IndexOf(sb[i]);
                if (chi >= 0)
                    sb[i] = b[chi];
            }
            return sb.ToString();
        }

        #endregion

        /// <summary>True when there is enough configuration to attempt an update.</summary>
        public bool IsConfigured
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Token))
                    return false;
                if (Domain == "*")
                    return Domains.Count > 0;
                return !string.IsNullOrWhiteSpace(Domain);
            }
        }

        /// <summary>Human friendly description of the configured domain(s).</summary>
        public string DomainSummary
        {
            get
            {
                if (Domain == "*")
                    return string.Join(", ", Domains.Select(d => d.Domain));
                return Domain ?? "";
            }
        }

        /// <summary>
        /// Parses interval strings like "30s", "5m", "1h", "1d". Returns null when invalid.
        /// </summary>
        public static TimeSpan? ParseInterval(string istr)
        {
            istr = (istr ?? "").Trim().ToLowerInvariant();
            int iint;
            if (istr.Length < 2 || !int.TryParse(istr.Substring(0, istr.Length - 1), out iint) || iint <= 0)
                return null;
            switch (istr[istr.Length - 1])
            {
                case 's': return TimeSpan.FromSeconds(iint);
                case 'm': return TimeSpan.FromMinutes(iint);
                case 'h': return TimeSpan.FromHours(iint);
                case 'd': return TimeSpan.FromDays(iint);
                default: return null;
            }
        }

        public string Domain { get; set; } = "";
        public string Token { get; set; } = "";
        public string Interval { get; set; } = "10m";
        public string ServiceURL { get; set; } = "https://www.duckdns.org/update?domains=<DOM>&token=<TKN>&ip=<IP4>&ipv6=<IP6>";
        public List<DDnsDomain> Domains { get; } = new List<DDnsDomain>();
    }

    class DDnsDomain
    {
        public string Domain { get; set; }
        public DDnsResolutionMode ResolutionMode { get; set; }
        public string ResolutionValue { get; set; }

        public string BindIP { get; set; }
    }

    enum DDnsResolutionMode
    {
        Server, Local, Fixed, Host, WebService
    }

    class DSON //DDns Object Notation ;)
    {
        private static void Serialize(object o, List<string> data, string pre = "")
        {
            if (o == null)
            {
                data.Add(":null");
                return;
            }
            Type t = o.GetType();
            PropertyInfo[] pis = t.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                if (pi.GetIndexParameters().Length > 0 || !pi.CanRead)
                    continue;
                // only serialize settable scalars (skip computed properties like IsConfigured) and lists
                object value = pi.GetValue(o);
                if (pi.PropertyType == typeof(string) && pi.CanWrite)
                    data.Add(pre + pi.Name + ":" + value);
                else if (pi.PropertyType == typeof(int) && pi.CanWrite)
                    data.Add(pre + pi.Name + ":" + value);
                else if (pi.PropertyType.IsEnum && pi.CanWrite)
                    data.Add(pre + pi.Name + ":" + value.ToString());
                else if (pi.PropertyType.IsArray)
                {
                    foreach (object i in (object[])value)
                    {
                        data.Add(pre + pi.Name + "[");
                        Serialize(i, data, pre + '\t');
                        data.Add("]");
                    }
                }
                else if (value is IList)
                    foreach (object i in (IList)value)
                    {
                        data.Add(pre + pi.Name + "[");
                        Serialize(i, data, pre + '\t');
                        data.Add("]");
                    }
            }
        }

        public static string[] Serialize(object o)
        {
            List<string> data = new List<string>();
            Serialize(o, data);
            return data.ToArray();
        }

        private static void DeserializeInto(ref object o, Queue<string> data)
        {
            while (data.Count > 0)
            {
                string line = data.Dequeue();
                if (line == "]")
                    return;
                if (line == ":null")
                {
                    o = null;
                    return;
                }
                Type t = o.GetType();
                int dp = line.IndexOf(':');
                string pname;
                string value;
                bool array;
                if (dp < 0)
                {
                    if (line.EndsWith("["))
                        pname = line.Substring(0, line.Length - 1);
                    else
                        throw new Exception("Line error: " + line);
                    value = "";
                    array = true;
                }
                else
                {
                    pname = line.Substring(0, dp);
                    value = line.Substring(dp + 1);
                    array = false;
                }
                pname = pname.Trim();
                PropertyInfo pi = t.GetProperty(pname);
                if (pi == null)
                    continue;
                if (array)
                {
                    Type itemtype;
                    IList list;
                    bool isPureArray;
                    bool created = false;
                    if (pi.PropertyType.IsArray)
                    {
                        itemtype = pi.PropertyType.GetElementType();

                        isPureArray = true;
                        list = new ArrayList((object[])pi.GetValue(o));
                    }
                    else if (typeof(IList).IsAssignableFrom(pi.PropertyType))
                    {
                        itemtype = pi.PropertyType.GetGenericArguments()[0];
                        isPureArray = false;
                        list = (IList)pi.GetValue(o);
                        if (list == null)
                        {
                            list = new ArrayList();
                            created = true;
                        }
                    }
                    else throw new Exception("Unsupported list type");
                    object itemo = Activator.CreateInstance(itemtype);
                    DeserializeInto(ref itemo, data);
                    list.Add(itemo);
                    if (isPureArray)
                    {
                        Array a = Array.CreateInstance(itemtype, list.Count);
                        list.CopyTo(a, 0);
                        pi.SetValue(o, a);
                    }
                    else if (created)
                    {
                        pi.SetValue(o, list);
                    }
                }
                else
                {
                    if (!pi.CanWrite)
                        continue;
                    if (pi.PropertyType == typeof(string))
                        pi.SetValue(o, value);
                    if (pi.PropertyType == typeof(int))
                        pi.SetValue(o, int.Parse(value));
                    if (pi.PropertyType.IsEnum)
                        pi.SetValue(o, Enum.Parse(pi.PropertyType, value));
                }
            }
        }

        public static void DeserializeInto(object o, string[] data)
        {
            Queue<string> q = new Queue<string>(data);
            DeserializeInto(ref o, q);
        }
    }
}
