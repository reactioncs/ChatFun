﻿using System.Net;

namespace ChatCommon
{
    public class ReadConfig
    {
        public static bool ReadAdress(string path, out IPAddress? address, out int port)
        {
            port = 0;

            using (StreamReader sw = new(path))
            {
                if (!IPAddress.TryParse(sw.ReadLine(), out address))
                    return false;
                if(!int.TryParse(sw.ReadLine(), out port))
                    return false;
            }

            return true;
        }
    }
}
