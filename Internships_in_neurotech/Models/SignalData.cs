using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Internships_in_neurotech.Models
{
    internal class SignalData
    {
        public double[][] DataFromFile;
        public List<List<double>> AverageValue;
        public List<List<double>> MinValue;
        public List<List<double>> MaxValue;

        private Task[] tasks;

        /// <summary>
        /// Класс реализует обработку сигналов из файлов, названия которых указаны в файле MethDescription.xml
        /// поэтому создание класса невозможно без передачи этих данных
        /// </summary>
        /// <param name="serializedChannel"></param>
        public SignalData(in SerializedChannel serializedChannel)
        {
            int channelsCount = serializedChannel.bosMeth!.Channels!.Count;

            DataFromFile = new double[channelsCount][]; //new List<List<double>>(channelsCount);

            AverageValue = new List<List<double>>(channelsCount);
            MinValue = new List<List<double>>(channelsCount);
            MaxValue = new List<List<double>>(channelsCount);

            for (int i = 0; i < channelsCount; i++)
            {
                AverageValue.Add(new List<double>());
                MinValue.Add(new List<double>());
                MaxValue.Add(new List<double>());
            }

            tasks = new Task[channelsCount];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = GetDataAsync(serializedChannel.DirectoryPath + serializedChannel.bosMeth.Channels[i].SignalFileName,
                i, serializedChannel.bosMeth.Channels[i].EffectiveFd);
            }

            //Task.WaitAll(tasks, TimeSpan.FromMilliseconds(5));  
            Waiting();
        }

        // Функция для блокировки потока (чтобы у пользователей со слабыми устройствами не возникало ошибок)
        private async void Waiting()
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                await tasks[i];
            }
        }

        //                   полный путь и название файла   индекс потока   частота дискретизации 
        private async Task GetDataAsync(string signalPath, int index, int sizeOfTicks)
        {
            await Task.Run(() =>
            {
                try
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
                            int i;


                            signalFile.Read(currentPartOfFile, 0, sizeOfTicks * sizeof(double));

                            // Если длинна файла больше суммы числа уже обработанных тиков и тех, то будут обработаны сейчас, то...
                            if (DataFromFile[index].Length > previousTicks + sizeOfTicks)
                            {
                                for (i = 0; i < sizeOfTicks; i++)
                                    // конвертация части файла,которую считали в эту итерацию                         так как файл в byte, то смещаем указатель на длину нужного нам типа (double)
                                    DataFromFile[index][i + previousTicks] = BitConverter.ToDouble(currentPartOfFile, i * sizeof(double));
                            }
                            else
                            {
                                for (i = 0; i < (DataFromFile[index].Length - previousTicks); i++)
                                    // конвертация части файла,которую считали в эту итерацию
                                    DataFromFile[index][i + previousTicks] = BitConverter.ToDouble(currentPartOfFile, i * sizeof(double));
                            }

                            // Выделение фрагмента массива, в который мы заполнили значения в эту итерацию
                            double[] partOfSignal = DataFromFile[index][previousTicks..(previousTicks + i)];

                            AverageValue[index].Add(partOfSignal.Average());
                            MinValue[index].Add(partOfSignal.Min());
                            MaxValue[index].Add(partOfSignal.Max());

                            previousTicks += sizeOfTicks;
                        }

                        stopWatch.Stop();
                        Debug.WriteLine($"{index} --- {stopWatch.Elapsed}");
                    };
                }
                catch
                {
                    DataFromFile[index] = new double[] { 0 };
                    AverageValue[index].Add(0);
                    MinValue[index].Add(0);
                    MaxValue[index].Add(0);
                    Debug.WriteLine($"{signalPath} doesn't found");
                }
            });
        }
    }
}
