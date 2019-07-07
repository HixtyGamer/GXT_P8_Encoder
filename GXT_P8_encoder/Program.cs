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
            byte[] image = File.ReadAllBytes(path);
            Console.WriteLine("Path to original GXT image");
            string pathGXT = Console.ReadLine();
            Console.WriteLine("Path to save re-encrypted GXT image");
            string pathEnd = Console.ReadLine();
            Console.WriteLine("Generate palette? (1 or 0)");
            string palette_ch = Console.ReadLine();
            Console.WriteLine("Processing...");
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
                    int[,] palette_gen = new int[16000000, 5];
                    for (int n = 0; n < image.Length; n+=4 )
                    {
                        int Rp = image[n];
                        int Gp = image[n + 1];
                        int Bp = image[n + 2];
                        int Ap = image[n + 3];
                        int count = 0;
                        bool check = false;
                        for (int x = 0; x < n/4; x++)
                        {
                            if (Rp == palette_gen[x,0] && Gp == palette_gen[x,1] && Bp == palette_gen[x,2] && Ap == palette_gen[x,3])
                            {
                                check = true;
                                break;
                            }
                            if (palette_gen[x, 4] == 0) break;
                        }
                        if (check) continue;
                        for (int y = 0; y < image.Length; y += 4)
                        {
                            if  (Rp == image[y] && Gp == image[y+1] && Bp == image[y+2] && Ap == image[y + 3])
                            {
                                count++;
                            }
                        }
                        for (int z = 0; z <= n/4; z++)
                        {
                            if (count > palette_gen[z,4])
                            {
                                int[] save = new int[5];
                                save[0] = palette_gen[z,0];
                                save[1] = palette_gen[z, 1];
                                save[2] = palette_gen[z, 2];
                                save[3] = palette_gen[z, 3];
                                save[4] = palette_gen[z, 4];
                                palette_gen[z, 0] = Rp;
                                palette_gen[z, 1] = Gp;
                                palette_gen[z, 2] = Bp;
                                palette_gen[z, 3] = Ap;
                                palette_gen[z, 4] = count;
                                Rp = save[0];
                                Gp = save[1];
                                Bp = save[2];
                                Ap = save[3];
                                count = save[4];
                            }
                            if (count == 0) break; 
                        }
                        if (n % 100 == 0)
                        Console.WriteLine("{0} of {1}",n,image.Length);
                    }
                    for (int x = 0; x < 256; x++)
                    {
                        palette[x * 4] = Convert.ToByte(palette_gen[x, 2]);
                        palette[x * 4 + 1] = Convert.ToByte(palette_gen[x, 1]);
                        palette[x * 4 + 2] = Convert.ToByte(palette_gen[x, 0]);
                        palette[x * 4 + 3] = Convert.ToByte(palette_gen[x, 3]);
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
                        // Console.WriteLine("Произошла замена: {0} {1} {2} {3} на {4} {5} {6} {7}", image[n], image[n + 1], image[n + 2], image[n + 3], paletteARGB[num, 1], paletteARGB[num, 2], paletteARGB[num, 3], paletteARGB[num, 0]);
                        
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
