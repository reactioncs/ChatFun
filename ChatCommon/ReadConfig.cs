using System.Net;

namespace ChatCommon
{
    public class ReadConfig
    {
        public static bool ReadAddress(string path, out IPAddress address, out int port)
        {
            port = 0;

            using (StreamReader sw = new(path))
            {
                if (!IPAddress.TryParse(sw.ReadLine(), out IPAddress? addressParse))
                {
                    address = IPAddress.None;
                    return false;
                }
                else
                {
                    address = addressParse;
                }

                if (!int.TryParse(sw.ReadLine(), out port))
                    return false;
            }

            return true;
        }
    }
}
