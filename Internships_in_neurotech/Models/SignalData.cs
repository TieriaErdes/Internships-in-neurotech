using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Internships_in_neurotech.Models
{
    internal class SignalData
    {
        public double[][] DataFromFile;

        public double[][] averageValue;
        public double[][] minValue;
        public double[][] maxValue;

        public double[] absolutAverageValue;
        public double[] absolutMinValue;
        public double[] absolutMaxValue;

        private Task[] tasks;
        private const int tickLength = 1000;

        /// <summary>
        /// Класс реализует обработку сигналов из файлов, названия которых указаны в файле MethDescription.xml
        /// поэтому создание класса невозможно без передачи этих данных
        /// </summary>
        /// <param name="serializedChannel"></param>
        public SignalData(in SerializedChannel serializedChannel)
        {
            int channelsCount = serializedChannel.bosMeth!.Channels!.Count;

            DataFromFile = new double[channelsCount][];

            averageValue = new double[channelsCount][];
            minValue = new double[channelsCount][];
            maxValue = new double[channelsCount][];

            absolutAverageValue = new double[channelsCount];
            absolutMaxValue = new double[channelsCount];
            absolutMinValue = new double[channelsCount];

            Waiting(serializedChannel);
        }

        private async void Waiting(SerializedChannel serializedChannel)
        {
            tasks = new Task[serializedChannel.bosMeth!.Channels!.Count];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = GetDataAsync(Path.Combine(serializedChannel.DirectoryPath!, serializedChannel.bosMeth.Channels[i].SignalFileName!),
                i, serializedChannel.bosMeth.Channels[i].EffectiveFd);
            }

            await Task.WhenAll(tasks);
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


                        // Если размер файла меньше чем десять окон, которыми ведётся отсчёт, то...
                        if (signalFile.Length < (tickLength * 10 * sizeof(double)))
                            // В соответствующей файлу строке выделяется массив памяти равный количеству чисел double в нём
                            DataFromFile[index] = new double[signalFile.Length / sizeof(double)];
                        else
                            DataFromFile[index] = new double[tickLength * 10];


                        averageValue[index] = new double[signalFile.Length / sizeof(double) / tickLength + 1];
                        minValue[index] = new double[signalFile.Length / sizeof(double) / tickLength + 1];
                        maxValue[index] = new double[signalFile.Length / sizeof(double) / tickLength + 1];

                        // Фрагмент файла длинной 1000 тиков double
                        byte[] currentPartOfFile = new byte[tickLength * sizeof(double)];
                        // Счётчик сколько тиков прошло
                        int previousTicks = 0;

                        // Чтение файла фрагментами
                        while (signalFile.Position < signalFile.Length)
                        {
                            int i;

                            signalFile.Read(currentPartOfFile, 0, tickLength * sizeof(double));

                            //Если длинна файла больше суммы числа уже обработанных тиков и тех, то будут обработаны сейчас, то...
                            if (DataFromFile[index].Length > previousTicks + tickLength)
                            {
                                for (i = 0; i < tickLength; i++)
                                    // конвертация части файла,которую считали в эту итерацию                         так как файл в byte, то смещаем указатель на длину нужного нам типа (double)
                                    DataFromFile[index][i + previousTicks] = BitConverter.ToDouble(currentPartOfFile, i * sizeof(double));
                            }
                            else
                            {
                                for (i = 0; i < (DataFromFile[index].Length - previousTicks); i++)
                                    // конвертация части файла,которую считали в эту итерацию
                                    DataFromFile[index][i + previousTicks] = BitConverter.ToDouble(currentPartOfFile, i * sizeof(double));
                            }

                            //Выделение фрагмента массива, в который мы заполнили значения в эту итерацию
                            double[] partOfSignal = DataFromFile[index][previousTicks..(previousTicks + i)];


                            averageValue[index][previousTicks / tickLength] = partOfSignal.Average();
                            minValue[index][previousTicks / tickLength] = partOfSignal.Min();
                            maxValue[index][previousTicks / tickLength] = partOfSignal.Max();


                            previousTicks += tickLength;
                        }

                        stopWatch.Stop();
                        Debug.WriteLine($"{index} --- {stopWatch.Elapsed}");
                    };

                    absolutAverageValue[index] = averageValue[index].Average();
                    absolutMaxValue[index] = averageValue[index].Max();
                    absolutMinValue[index] = averageValue[index].Min();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}");

                    DataFromFile[index] = new double[] { 0 };
                    averageValue[index] = new double[] { 0 };
                    minValue[index] = new double[] { 0 };
                    maxValue[index] = new double[] { 0 };
                    Debug.WriteLine($"{signalPath} doesn't found");
                }
            });
        }
    }
}
