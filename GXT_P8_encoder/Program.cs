using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GXT_P8_encoder
{
    class Program
    {
        static void Main(string[] args)
        {
        start:
            Console.WriteLine("Path to Photoshop RAW image");
            string path = Console.ReadLine();           
            Console.WriteLine("Path to original GXT image");
            string pathGXT = Console.ReadLine();
            Console.WriteLine("Path to save re-encrypted GXT image");
            string pathEnd = Console.ReadLine();
            Console.WriteLine("Generate palette? (1 or 0)");
            string palette_ch = Console.ReadLine();
            Console.WriteLine("Processing...");
            byte[] image = File.ReadAllBytes(path);
            using (FileStream fstream = new FileStream(pathGXT, FileMode.Open))
            {
                byte[] header = new byte[64];
                byte[] palette = new byte[1024];
                byte[] full = new byte[fstream.Length];
                byte[,] paletteARGB = new byte[256, 4];
                fstream.Read(header, 0, 64);
                int r = 0;
                if (palette_ch == Convert.ToString(1))
                {
                    List<List<byte>> palette_gen = new List<List<byte>>();
                    List<int> palette_count = new List<int>();
                    for (int n = 0; n < image.Length; n += 4)
                    {
                        List<byte> color = new List<byte>();
                        color.Add(image[n]);
                        color.Add(image[n + 1]);
                        color.Add(image[n + 2]);
                        color.Add(image[n + 3]);
                        if (n % 1000 == 0 || n == image.Length - 4)
                            Console.WriteLine("{0} of {1}", n, image.Length - 4);
                        for (int x = 0; x < palette_gen.Count + 1; x++)
                        {
                            try
                            {
                                if (color[0] == palette_gen[x][0] && color[1] == palette_gen[x][1] && color[2] == palette_gen[x][2] && color[3] == palette_gen[x][3])
                                {
                                    palette_count[x]++;
                                    break;
                                }
                            }
                            catch
                            {
                                palette_gen.Add(color);
                                palette_count.Add(1);
                                break;
                            }
                        }
                    }
                    for (int z = 0; z < 256; z++)
                    {
                        int index = z;
                        for (int m = z + 1; m < palette_gen.Count; m++)
                            if (palette_count[m] > palette_count[index]) index = m;
                        byte[] save = new byte[4];
                        int cnt_sv = 0;
                        save[0] = palette_gen[z][0];
                        save[1] = palette_gen[z][1];
                        save[2] = palette_gen[z][2];
                        save[3] = palette_gen[z][3];
                        cnt_sv = palette_count[z];
                        palette_gen[z][0] = palette_gen[index][0];
                        palette_gen[z][1] = palette_gen[index][1];
                        palette_gen[z][2] = palette_gen[index][2];
                        palette_gen[z][3] = palette_gen[index][3];
                        palette_count[z] = palette_count[index];
                        palette_gen[index][0] = save[0];
                        palette_gen[index][1] = save[1];
                        palette_gen[index][2] = save[2];
                        palette_gen[index][3] = save[3];
                        palette_count[index] = cnt_sv;
                    }
                    for (int x = 0; x < 256; x++)
                    {
                        palette[x * 4] = Convert.ToByte(palette_gen[x][2]);
                        palette[x * 4 + 1] = Convert.ToByte(palette_gen[x][1]);
                        palette[x * 4 + 2] = Convert.ToByte(palette_gen[x][0]);
                        palette[x * 4 + 3] = Convert.ToByte(palette_gen[x][3]);
                    }
                    Console.WriteLine("Palette generated.");
                    Console.WriteLine("Still processing...");
                }
                else
                {
                    fstream.Seek(-1024, SeekOrigin.End);
                    fstream.Read(palette, 0, 1024);
                }
                for (int n = 0; n < 64; n++) full[n] = header[n];
                for (int n = 0; n < 256; n++)
                {
                    for (int v = 3; v >= 0; v--)
                    {
                        paletteARGB[n, v] = palette[r];
                        r++;
                    }
                }
                for (int n = 0; n < (fstream.Length - 1088) * 4; n += 4)
                {
                    bool check = true;
                    for (int v = 0; v < 256; v++)
                        if (image[n] == paletteARGB[v, 1] && image[n + 1] == paletteARGB[v, 2] && image[n + 2] == paletteARGB[v, 3] && image[n + 3] == paletteARGB[v, 0])
                        {
                            full[n / 4 + 64] = (byte)v;
                            check = false;
                        }
                    int R, G, B, A;
                    int num = 0, ch = 1024;

                    if (check)
                    {
                        if (image[n + 3] <= 3)
                        {
                            for (int o = 0; o < 256; o++)
                            {
                                if (paletteARGB[o, 0] <= 2)
                                {
                                    num = o;
                                    break;
                                }
                            }
                        }
                        else
                            for (int o = 0; o < 256; o++)
                            {
                                R = Math.Abs(image[n] - paletteARGB[o, 1]);
                                G = Math.Abs(image[n + 1] - paletteARGB[o, 2]);
                                B = Math.Abs(image[n + 2] - paletteARGB[o, 3]);
                                A = Math.Abs(image[n + 3] - paletteARGB[o, 0]);
                                if ((R + G + B + A) < ch)
                                {
                                    ch = R + G + B + A;
                                    num = o;
                                }
                            }                      
                        full[n / 4 + 64] = (byte)num;
                    }
                }
                for (int k = 0; k < 1024; k++)
                    full[full.Length - 1024 + k] = palette[k];
                File.WriteAllBytes(pathEnd, full);
            }
            Console.WriteLine("Done");
            Console.ReadKey();
            goto start;
        }
    }
}
