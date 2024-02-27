using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Internships_in_neurotech.Models
{
    internal sealed class SignalData
    {
        public double[][] DataFromFile;

        public double[][] averageValue;
        public double[][] maxValue;
        public double[][] minValue;

        private double[] _absoluteAverageValue;
        private double[] _absoluteMinValue;
        private double[] _absoluteMaxValue;

        private Task[]? _tasks;
        private const int _tickLength = 1000;

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

            _absoluteAverageValue = new double[channelsCount];
            _absoluteMaxValue = new double[channelsCount];
            _absoluteMinValue = new double[channelsCount];

            StartTasks(serializedChannel);
        }

        private async void StartTasks(SerializedChannel serializedChannel)
        {
            _tasks = new Task[serializedChannel.bosMeth!.Channels!.Count];
            for (int i = 0; i < _tasks.Length; i++)
            {
                _tasks[i] = GetDataAsync(Path.Combine(serializedChannel.DirectoryPath!, serializedChannel.bosMeth.Channels[i].SignalFileName!), i);
            }

            await Task.WhenAll(_tasks);
        }


        //                   полный путь и название файла   индекс потока   частота дискретизации 
        private async Task GetDataAsync(string signalPath, int index)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (FileStream signalFile = new FileStream(signalPath, FileMode.Open))
                    {
                        // Просто таймер для засечения достаточности быстродействия алгоритма
                        var stopWatch = Stopwatch.StartNew();

                        long fileLength = signalFile.Length / sizeof(double);

                        // Если размер файла меньше чем сто окон (1000 единиц), которыми ведётся отсчёт, то...
                        if (signalFile.Length < (_tickLength * 100 * sizeof(double)))
                        {
                            averageValue[index] = new double[fileLength / _tickLength + 1];
                            minValue[index] = new double[fileLength / _tickLength + 1];
                            maxValue[index] = new double[fileLength / _tickLength + 1];

                            // В соответствующей файлу строке выделяется массив памяти равный количеству чисел double в нём
                            DataFromFile[index] = new double[fileLength];

                            byte[] currentPartOfFile = new byte[_tickLength * sizeof(double)];
                            // Счётчик сколько тиков прошло
                            int previousTicks = 0;

                            int i;
                            // Чтение файла фрагментами
                            while (signalFile.Position < signalFile.Length)
                            {

                                signalFile.Read(currentPartOfFile, 0, _tickLength * sizeof(double));

                                //Если длинна файла больше суммы числа уже обработанных тиков и тех, то будут обработаны сейчас, то...
                                if (DataFromFile[index].Length > previousTicks + _tickLength)
                                {
                                    for (i = 0; i < _tickLength; i++)
                                        // конвертация части файла,которую считали в эту итерацию
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

                                averageValue[index][previousTicks / _tickLength] = partOfSignal.Average();
                                minValue[index][previousTicks / _tickLength] = partOfSignal.Min();
                                maxValue[index][previousTicks / _tickLength] = partOfSignal.Max();


                                previousTicks += _tickLength;
                            }


                            _absoluteAverageValue[index] = averageValue[index].Average();
                            _absoluteMaxValue[index] = averageValue[index].Max();
                            _absoluteMinValue[index] = averageValue[index].Min();
                        }

                        // Если файл большого размера
                        /// (хранение значений для выведения пользователю не предусмотрено с целью сокращения жизни переменных)
                        else 
                        {
                            // использование дополнительных переменных массивов,чтобы уменьшить продолжительность жизни массивов (garbage collector)
                            double[] _averageValue = new double[fileLength / _tickLength + 1];
                            double[] _minValue = new double[fileLength / _tickLength + 1];
                            double[] _maxValue = new double[fileLength / _tickLength + 1];

                            byte[] currentPartOfFile = new byte[_tickLength * sizeof(double)];
                            // Счётчик сколько тиков прошло
                            long previousTicks = 0;

                            double[] partOfSignal;

                            int i;
                            // Чтение файла фрагментами
                            while (signalFile.Position < signalFile.Length)
                            {
                                partOfSignal = new double[_tickLength * 100];

                                signalFile.Read(currentPartOfFile, 0, _tickLength * sizeof(double));

                                //Если длинна файла больше суммы числа уже обработанных тиков и тех, то будут обработаны сейчас, то...
                                if (fileLength > previousTicks + _tickLength)
                                {
                                    for (i = 0; i < _tickLength; i++)
                                        // конвертация части файла,которую считали в эту итерацию 
                                        partOfSignal[i + previousTicks] = BitConverter.ToDouble(currentPartOfFile, i * sizeof(double));
                                }
                                else
                                {
                                    for (i = 0; i < (fileLength - previousTicks); i++)
                                        // конвертация части файла,которую считали в эту итерацию
                                        partOfSignal[i + previousTicks] = BitConverter.ToDouble(currentPartOfFile, i * sizeof(double));
                                }

                                // Так как каждую итерацию мы заново выделяем память, то нет нужды выделять какой-то фрагмент (в отличии от случая выше)
                                averageValue[index][previousTicks / _tickLength] = partOfSignal.Average();
                                minValue[index][previousTicks / _tickLength] = partOfSignal.Min();
                                maxValue[index][previousTicks / _tickLength] = partOfSignal.Max();


                                previousTicks += _tickLength;
                            }

                            _absoluteAverageValue[index] = _averageValue.Average();
                            _absoluteMaxValue[index] = _averageValue.Max();
                            _absoluteMinValue[index] = _averageValue.Min();
                        }

                        stopWatch.Stop();
                        Debug.WriteLine($"{index} --- {stopWatch.Elapsed}");
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}");

                    DataFromFile[index] = new double[] { 0 };
                    averageValue[index] = new double[] { 0 };
                    minValue[index] = new double[] { 0 };
                    maxValue[index] = new double[] { 0 };
                }
            });
        }
    }
}
