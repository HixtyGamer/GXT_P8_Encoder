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
            Console.WriteLine("Путь к RAW файлу");
            string path = Console.ReadLine();
            byte[] image = File.ReadAllBytes(path);
            Console.WriteLine("Путь к оригинальному GXT файлу");
            string pathGXT = Console.ReadLine();
            Console.WriteLine("Путь к будущему GXT файлу");
            string pathEnd = Console.ReadLine();
            Console.WriteLine("Ну жди теперь...");
            using (FileStream fstream = new FileStream(pathGXT, FileMode.Open))
            {
                byte[] header = new byte[64];
                byte[] palette = new byte[1024];
                byte[,] paletteARGB = new byte[256, 4];
                byte[] full = new byte[fstream.Length];
                int r = 0;
                fstream.Read(header, 0, 64);
                fstream.Seek(-1024, SeekOrigin.End);
                fstream.Read(palette, 0, 1024);

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
                                if (paletteARGB[o, 0] == 0)
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
            Console.WriteLine("Вроде как всё");
            Console.ReadKey();
        }
    }
}
