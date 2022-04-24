using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace shred
{
    internal class Program
    {
        static void Main(string[] args) {
            int Passes = 2;
            string vFA = args[0], vFA2 = vFA.ToLower();
            if ("?" == vFA2 || "/?" == vFA2) {
                Console.WriteLine("--- Command Line examples ---");
                Console.WriteLine("\r\nshred file.txt");
                Console.WriteLine("- Overwrites 2 times then delete.");
                Console.WriteLine("\r\nshred file.txt 3");
                Console.WriteLine("- Overwrites 3 times then delete.");
                Console.WriteLine("\r\nshred file.txt 11 logfile.log");
                Console.WriteLine("- Overwrites 11 times. Logs File List to logfile.log");
                return;
            }
            if (args.Length > 1) 
                int.TryParse(args[1], out Passes);
            string[] vItems = new string[] { vFA };
            bool vFlag1 = Directory.Exists(vFA);
            bool vFlag2 = File.Exists(vFA);
            if (vFlag1 && !vFlag2) {
                Console.WriteLine("Retrieving File List from Directory . . .");
                vItems = GetItems(vFA);
            }
            if (args.Length > 2)
                File.WriteAllText(args[2], string.Join("\r\n", vItems));
            Console.WriteLine("Shredding . . .");
            var vDL = new List<string>();
            foreach (string item in vItems) {
                Console.Title = "Shredding: " + item;
                if (Directory.Exists(item))
                    vDL.Add(item);
                else for (int i = 0; i < Passes; i++) {
                    FileStream vFS = null;
                    try {
                        vFS = new FileStream(item, FileMode.Open);
                        var vLen = vFS.Length;
                        int vBufL = 1024;
                        double vPiecesDbl = (double)vLen / (double)vBufL;
                        double vPiecesLastDbl = vPiecesDbl % 1.0;
                        long vPiecesLng = (long)vPiecesDbl;
                        int vRemainingBytes = (int)Math.Ceiling(vPiecesLastDbl * vBufL);
                        byte[] vBytes;
                        for (long vi = 0L; vi < vPiecesLng; vi++) {
                            vBytes = GetBytes(vBufL);
                            vFS.Write(vBytes, 0, vBufL);
                        }
                        if (vRemainingBytes > 0) {
                            vBytes = GetBytes(vRemainingBytes);
                            vFS.Write(vBytes, 0, vBytes.Length);
                        }
                    } catch (Exception ex) {
                        if (null != vFS) {
                            vFS.Close(); vFS.Dispose();
                        }
                        break; 
                    }
                    if (null != vFS) {
                        vFS.Close(); vFS.Dispose();
                    }
                    try { File.Delete(item); } catch { }
                }
            }
            foreach (string vDir in vDL)
                try { Directory.Delete(vDir, true); } catch { }
            Console.WriteLine("Done.");
        }
        static string[] GetItems(string aDirectory) {
            var vItems = new List<string>();
            vItems.Add(aDirectory);
            foreach (string vItem in Directory.GetDirectories(aDirectory))
                vItems.AddRange(GetItems(vItem));
            foreach (string vItem in Directory.GetFiles(aDirectory))
                vItems.Add(vItem);
            return vItems.ToArray();
        }
        static Random m_R = new Random();
        static byte[] GetBytes(int aLen) {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < aLen; i++)
                bytes.Add((byte)m_R.Next(0, 254));
            return bytes.ToArray();
        }
    }
}
