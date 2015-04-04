using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SharpMat.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var stream = File.Open("C:\\Users\\Daniel\\Matlab\\testing.mat", FileMode.Open))
            {
                var reader = new MatReader(stream);
                var header = reader.ReadHeader();
                var tag = reader.ReadTag();
                var data = reader.ReadData();
            }
        }
    }
}
