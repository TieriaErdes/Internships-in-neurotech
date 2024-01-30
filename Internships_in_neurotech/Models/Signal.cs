using Avalonia.Controls.Converters;
using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Internships_in_neurotech.Models
{
    internal class Signal
    {
        private int sizeOfTicks = 1000;

        public double[][] DataFromFile;
        public Task[] tasks;

        /// <summary>
        /// Класс реализует обработку сигналов из файлов, названия которых указаны в файле MethDescription.xml
        /// поэтому создание класса невозможно без передачи этих данных
        /// </summary>
        /// <param name="serializedChannel"></param>
        public Signal(in SerializedChannel serializedChannel)
        {
            DataFromFile = new double[serializedChannel.bosMeth.Channels.Capacity][];

            tasks = new Task[serializedChannel.bosMeth.Channels.Capacity];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = GetDataAsync(serializedChannel.FilePath + serializedChannel.bosMeth.Channels[i].SignalFileName,
                    i, serializedChannel.bosMeth.Channels[i].EffectiveFd);
            }

            //Task.WaitAll(tasks, TimeSpan.FromMilliseconds(1));     КОСТЫЛЬ
            //Debug.WriteLine("All tasks complete successful");

        }

        //                   полный путь и название файла   индекс потока   частота дискретизации 
        private async Task GetDataAsync(string signalPath, int index, int sizeOfTicks)
        {
            await Task.Run(() =>
            {
                using (FileStream signalFile = new FileStream(signalPath, FileMode.Open))
                { 
                    // Просто таймер для засечения достаточности быстродействия алгоритма
                    var stopWatch = Stopwatch.StartNew();

                    // В соответствующей файлу строке выделяется массив памяти равный количеству чисел double в нём
                    DataFromFile[index] = new double[signalFile.Length / sizeof(double)];

                    // Фрагмент файла длинной 1000 тиков double
                    byte[] currentPartOfFile = new byte[sizeOfTicks * sizeof(double)];
                    // Счётчик сколько тиков прошло
                    int previousTicks = 0;

                    while (signalFile.Position < signalFile.Length)
                    {
                        signalFile.Read(currentPartOfFile, 0, sizeOfTicks * sizeof(double));

                        // Если длинна файла больше суммы числа уже обработанных тиков и тех, то будут обработаны сейчас, то...
                        if (DataFromFile[index].Length > previousTicks + sizeOfTicks)
                        {
                            for (int i = 0; i < sizeOfTicks; i++)
                            {
                                // конвертация части файла,которую считали в эту итерацию                         так как файл в byte, то смещаем указатель на длину нужного нам типа (double)
                                DataFromFile[index][i + previousTicks] = BitConverter.ToDouble(currentPartOfFile, i * sizeof(double));
                            }
                            previousTicks += sizeOfTicks;
                        }
                        else
                            for (int i = 0; i < (DataFromFile[index].Length - previousTicks); i++)
                            {
                                // конвертация части файла,которую считали в эту итерацию
                                DataFromFile[index][i + previousTicks] = BitConverter.ToDouble(currentPartOfFile, i * sizeof(double));
                            }
                    }

                    stopWatch.Stop();
                    Debug.WriteLine($"{index} --- {stopWatch.Elapsed}");
                };
            });
        }
    }
}
