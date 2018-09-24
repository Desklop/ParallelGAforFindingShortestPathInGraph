using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace ParallelGAforFindingShortestPathInGraph
{
    struct Subpopulation
    {
        public int[,] individuals;
        public int[,] allFitnesses;
        public int numberThread;
    }
    class Program
    {
        static int countIndivids, countComp, countGenes, firstComp, lastComp;
        static int[,] matrWeidths;
        static List<int[]> migratoryIndividuals;
        static Subpopulation[] dataSubpopulations;
        static int countIndividsSubpopulation, periodMigration, countSubpopulation;
        static Semaphore semaphorSubpopulation, mainSemaphor;
        static bool exit = false;

        static void Main(string[] args)
        {
            StreamWriter writer = new StreamWriter("D://Для учёбы/4 семестр/Лабы по МРЗвИС/Лаба 7/исходная популяция.txt");
            countComp = 10;
            Console.WriteLine("Кол-во компьютеров: " + countComp);
            writer.WriteLine("Кол-во компьютеров: " + countComp);
            Random rand = new Random();

            matrWeidths = new int[countComp, countComp];
            for (int i = 0; i < countComp; i++)
                for (int j = i + 1; j < countComp; j++)
                    matrWeidths[i, j] = rand.Next(1, 99);
            for (int i = 0; i < countComp; i++)
                for (int j = 0; j < i; j++)
                    matrWeidths[i, j] = matrWeidths[j, i];

            Console.WriteLine("Длина пути от одного компьютера до другого: ");
            writer.WriteLine("Длина пути от одного компьютера до другого: ");
            Console.Write("   | ");
            writer.Write("   | ");
            for (int i = 0; i < countComp; i++)
            {
                if (i < 9)
                {
                    Console.Write("{0}   ", i + 1);
                    writer.Write("{0}   ", i + 1);
                }
                else
                {
                    Console.Write("{0}  ", i + 1);
                    writer.Write("{0}  ", i + 1);
                }
            }
            Console.WriteLine();
            writer.WriteLine();
            Console.Write("---");
            writer.Write("---");
            for (int i = 0; i < countComp; i++)
            {
                Console.Write("----");
                writer.Write("----");
            }
            Console.WriteLine();
            writer.WriteLine();
            for (int i = 0; i < countComp; i++)
            {
                if (i < 9)
                {
                    Console.Write("{0}  | ", i + 1);
                    writer.Write("{0}  | ", i + 1);
                }
                else
                {
                    Console.Write("{0} | ", i + 1);
                    writer.Write("{0} | ", i + 1);
                }
                for (int j = 0; j < countComp; j++)
                {
                    if (matrWeidths[i, j] > 9)
                    {
                        Console.Write("{0}  ", matrWeidths[i, j]);
                        writer.Write("{0}  ", matrWeidths[i, j]);
                    }
                    else
                    {
                        Console.Write("{0}   ", matrWeidths[i, j]);
                        writer.Write("{0}   ", matrWeidths[i, j]);
                    }
                }
                Console.WriteLine();
                writer.WriteLine();
            }

            firstComp = rand.Next(1, countComp);
            lastComp = rand.Next(1, countComp);
            if (firstComp == lastComp)
                firstComp = rand.Next(1, countComp);
            Console.WriteLine("Компьютер-отправитель: {0}, компьютер-получатель: {1}", firstComp, lastComp);
            writer.WriteLine("Компьютер-отправитель: {0}, компьютер-получатель: {1}", firstComp, lastComp);

            countIndivids = 30;
            Console.WriteLine("Кол-во особей в популяции: " + countIndivids);
            writer.WriteLine("Кол-во особей в популяции: " + countIndivids);

            countGenes = 6;
            Console.WriteLine("Кол-во генов у особи: " + countGenes);
            writer.WriteLine("Кол-во генов у особи: " + countGenes);

            int[,] individuals = new int[countIndivids, countGenes];  //особи
            for (int i = 0; i < countIndivids; i++)
                for (int j = 0; j < countGenes; j++)
                    individuals[i, j] = rand.Next(0, countComp);

            int[,] allFitnesses = new int[countIndivids, 2];  //пригодность каждой особи и её номер
            Console.WriteLine();
            writer.WriteLine();
            Console.WriteLine("Поколение 1:");
            writer.WriteLine("Поколение 1:");
            for (int i = 0; i < countIndivids; i++)
            {
                int fitness = 0;
                fitness += matrWeidths[firstComp - 1, individuals[i, 0]];
                Console.Write("Особь {0}, её гены: ", i + 1);
                writer.Write("Особь {0}, её гены: ", i + 1);
                for (int j = 0; j < countGenes; j++)
                {
                    if (individuals[i, j] < 9)
                    {
                        Console.Write("{0},  ", individuals[i, j] + 1);
                        writer.Write("{0},  ", individuals[i, j] + 1);
                    }
                    else
                    {
                        Console.Write("{0}, ", individuals[i, j] + 1);
                        writer.Write("{0}, ", individuals[i, j] + 1);
                    }
                    if (j > 0)
                        fitness += matrWeidths[individuals[i, j - 1], individuals[i, j]];
                }
                fitness += matrWeidths[individuals[i, countGenes - 1], lastComp - 1];
                Console.WriteLine(" пригодность: " + fitness);
                writer.WriteLine(" пригодность: " + fitness);
                allFitnesses[i, 0] = i;
                allFitnesses[i, 1] = fitness;
            }
            Console.WriteLine();
            writer.WriteLine();

            countSubpopulation = 5;  //кол-во подпопуляций
            countIndividsSubpopulation = countIndivids / countSubpopulation;  //кол-во особей в каждой подпопуляции
            periodMigration = 9;  //период миграции

            migratoryIndividuals = new List<int[]>();  //мигрирующие особи
            for (int i = 0; i < countSubpopulation; i++)
            {
                migratoryIndividuals.Add(new int[countGenes]);
                for (int j = 0; j < countGenes; j++)
                    migratoryIndividuals[i][j] = -1;  //значение каждого гена по умолчанию -1
            }

            int counter = 0;
            dataSubpopulations = new Subpopulation[countSubpopulation];
            Thread subpopulationThread;
            semaphorSubpopulation = new Semaphore(countSubpopulation, countSubpopulation * 10);
            mainSemaphor = new Semaphore(0, countSubpopulation * 10);
            for (int i = 0; i < countSubpopulation; i++)
            {  //разбитие популяции на подпопуляции и отправка их в отдельные потоки
                Subpopulation data = new Subpopulation();
                data.numberThread = i;  //номер потока
                data.individuals = new int[countIndividsSubpopulation, countGenes];
                data.allFitnesses = new int[countIndividsSubpopulation, 2];
                for (int j = 0; j < countIndividsSubpopulation; counter++, j++)  //копирование особей и их пригодности
                {
                    for (int k = 0; k < countGenes; k++)
                        data.individuals[j, k] = individuals[counter, k];
                    data.allFitnesses[j, 0] = allFitnesses[counter, 0];
                    data.allFitnesses[j, 1] = allFitnesses[counter, 1];
                }
                subpopulationThread = new Thread(new ParameterizedThreadStart(ProgressSubpopulation));
                subpopulationThread.Start(data);
            }

            int iterations = 0;
            do
            {
                for (int i = 0; i < countSubpopulation; i++)  //ждём завершения периода миграции каждой подпопуляции
                    mainSemaphor.WaitOne();
                Console.WriteLine("Миграционный сезон {0}:", iterations + 1);
                counter = 0;
                for (int i = 0; i < countSubpopulation; i++)  //вывод результатов
                {
                    Console.WriteLine("Подпопуляция {0}:", dataSubpopulations[i].numberThread + 1);
                    dataSubpopulations[i].allFitnesses = new int[countIndividsSubpopulation, 2];
                    OutputPopulation(ref dataSubpopulations[i].allFitnesses, dataSubpopulations[i].individuals);
                    for (int j = 0; j < countIndividsSubpopulation; counter++, j++)
                    {
                        allFitnesses[counter, 0] = dataSubpopulations[i].allFitnesses[j, 0];
                        allFitnesses[counter, 1] = dataSubpopulations[i].allFitnesses[j, 1];
                    }
                }
                Console.WriteLine();

                counter = 1;
                for (int i = 1; i < countIndivids; i++)  //подсчёт кол-ва особей с одинаковой пригодностью для остановки алгоритма
                    if (allFitnesses[i, 1] == allFitnesses[0, 1])
                        counter++;
                if (counter >= countIndivids * 0.9)
                    break;

                semaphorSubpopulation.Release(countSubpopulation);  //продолжаем миграцию
                iterations++;
            } while (iterations < 50);
            exit = true;
            semaphorSubpopulation.Release(countSubpopulation);
            writer.WriteLine("...");
            writer.WriteLine();
            Console.WriteLine("Кол-во миграционных сезонов: " + (iterations + 1));
            writer.WriteLine("Кол-во миграционных сезонов: " + (iterations + 1));
            Console.Write("Найденный путь: " + firstComp);
            writer.Write("Найденный путь: " + firstComp);
            for (int i = 0; i < countGenes; i++)
            {
                Console.Write(" -> " + (dataSubpopulations.First().individuals[0, i] + 1));
                writer.Write(" -> " + (dataSubpopulations.First().individuals[0, i] + 1));
            }
            Console.WriteLine(" -> {0}, его длина: " + allFitnesses[0, 1], lastComp);
            writer.WriteLine(" -> {0}, его длина: " + allFitnesses[0, 1], lastComp);
            Console.WriteLine("Длина прямого пути от {0} компьютера к {1}: " + matrWeidths[firstComp - 1, lastComp - 1], firstComp, lastComp);
            writer.WriteLine("Длина прямого пути от {0} компьютера к {1}: " + matrWeidths[firstComp - 1, lastComp - 1], firstComp, lastComp);
            writer.Close();
            Console.ReadLine();
        }

        static void ProgressSubpopulation(object obj)  //развитие подпопуляции
        {
            Subpopulation data = (Subpopulation)obj;
            List<int[]> individuals = new List<int[]>();
            for (int i = 0; i < countIndividsSubpopulation; i++)  //копирование полученных особей
            {
                individuals.Add(new int[countGenes]);
                for (int j = 0; j < countGenes; j++)
                    individuals.Last()[j] = data.individuals[i, j];
            }
            int[,] allFitnesses = new int[countIndividsSubpopulation, 2];
            for (int i = 0; i < countIndividsSubpopulation; i++)  //копирование полученных пригодностей
            {
                allFitnesses[i, 0] = data.allFitnesses[i, 0] - data.numberThread * countIndividsSubpopulation;
                allFitnesses[i, 1] = data.allFitnesses[i, 1];
            }

            string fileName = "D://Для учёбы/4 семестр/Лабы по МРЗвИС/Лаба 7/подпопуляция " + (data.numberThread + 1) + ".txt";
            using (StreamWriter writer = new StreamWriter(fileName)) { }  //очистка файла перед записью

            Random rand = new Random();
            bool check = true;
            int countMigrationsSeasons = 0;
            while (true)
            {
                countMigrationsSeasons++;
                Thread.Sleep(100);
                semaphorSubpopulation.WaitOne();
                if (exit) break;
                int numIndivid = rand.Next(0, countIndividsSubpopulation - 1);  //номер особи, которая заменятеся полученной в результате миграци
                if (!check)
                    while (true)
                    {
                        Thread.Sleep(1);
                        if (migratoryIndividuals[data.numberThread][0] != -1)  //получаем особь
                        {
                            for (int i = 0; i < countGenes; i++)
                            {
                                individuals[numIndivid][i] = migratoryIndividuals[data.numberThread][i];
                                migratoryIndividuals[data.numberThread][i] = -1;
                            }
                            break;
                        }
                    }
                check = false;

                using (StreamWriter writer = new StreamWriter(fileName, true))  //запись всех данных в файл
                {
                    writer.WriteLine("МИГРАЦИОННЫЙ СЕЗОН {0}:", countMigrationsSeasons);
                }

                for (int i = 0; i < periodMigration; i++)  //развиваем подпопуляцию в соотв. с периодом миграции
                {
                    IterationSubpopulation(ref individuals, allFitnesses);

                    using (StreamWriter writer = new StreamWriter(fileName, true))  //запись всех данных в файл
                    {
                        writer.WriteLine("Подпоколение {0}:", i + 1);
                        for (int j = 0; j < countIndividsSubpopulation; j++)
                        {
                            int fitness = 0;
                            fitness += matrWeidths[firstComp - 1, individuals[j][0]];
                            writer.Write("Особь {0}, её гены: ", j + 1);
                            for (int k = 0; k < countGenes; k++)
                            {
                                if (individuals[j][k] < 9)
                                    writer.Write("{0},  ", individuals[j][k] + 1);
                                else writer.Write("{0}, ", individuals[j][k] + 1);
                                if (k > 0)
                                    fitness += matrWeidths[individuals[j][k - 1], individuals[j][k]];
                            }
                            fitness += matrWeidths[individuals[j][countGenes - 1], lastComp - 1];
                            writer.WriteLine(" пригодность: " + fitness);
                            allFitnesses[j, 0] = j;
                            allFitnesses[j, 1] = fitness;
                        }
                        writer.WriteLine();
                    }
                }

                numIndivid = rand.Next(0, countIndividsSubpopulation - 1);  //номер особи для отправки
                for (int i = 0; i < countGenes; i++)  //отправляем особь
                    migratoryIndividuals[(data.numberThread + 1) % countSubpopulation][i] = individuals[numIndivid][i];

                dataSubpopulations[data.numberThread].numberThread = data.numberThread;  //"возвращаем" новые особи
                dataSubpopulations[data.numberThread].individuals = new int[countIndividsSubpopulation, countGenes];
                for (int i = 0; i < countIndividsSubpopulation; i++)
                    for (int j = 0; j < countGenes; j++)
                        dataSubpopulations[data.numberThread].individuals[i, j] = individuals[i][j];

                mainSemaphor.Release();
            }
        }

        static void IterationSubpopulation(ref List<int[]> individuals, int[,] allFitnesses)  //один цикл развития подпуляции
        {
            do
            {
                int[] parent_1 = new int[countGenes];  //родители
                int[] parent_2 = new int[countGenes];
                //выбор родителей методом инбридинг
                Inbreeding(individuals, allFitnesses, ref parent_1, ref parent_2);

                int[] descendant_1 = new int[countGenes];  //потомоки
                int[] descendant_2 = new int[countGenes];
                //скрещивание с пом. дискретной рекомбинации
                CrossingDiscreteRecombination(parent_1, parent_2, ref descendant_1, ref descendant_2);

                individuals.Add(descendant_1);
                individuals.Add(descendant_2);
            } while (individuals.Count < countIndividsSubpopulation);  //формируем популяцию потомков

            MutationExchange(ref individuals);  //мутация обменом соседних генов
            TruncationSelection(ref individuals);  //отбор особей в новое поколение методом усечения
        }

        static void TruncationSelection(ref List<int[]> individuals)  //отбор усечением
        {
            Random rand = new Random();
            double T = 0.4;  //порог для отбора
            int fitness = 0;
            int[,] allFitnesses = new int[individuals.Count, 2];  //пригодность каждой особи и её номер
            for (int i = 0; i < individuals.Count; i++)  //вычисление пригодности каждой особи
            {
                fitness += matrWeidths[firstComp - 1, individuals[i][0]];
                for (int j = 1; j < countGenes; j++)
                    fitness += matrWeidths[individuals[i][j - 1], individuals[i][j]];
                fitness += matrWeidths[individuals[i][countGenes - 1], lastComp - 1];
                allFitnesses[i, 0] = i;
                allFitnesses[i, 1] = fitness;
                fitness = 0;
            }

            int[] temp = new int[individuals.Count];  //сортировка особей в порядке убывания их пригодности
            for (int i = 0; i < individuals.Count; i++)
                temp[i] = allFitnesses[i, 1];
            Array.Sort(temp);
            for (int i = 0; i < individuals.Count; i++)
                for (int j = 0; j < individuals.Count; j++)
                    if (temp[i] == allFitnesses[j, 1])
                    {
                        Swap(ref allFitnesses[i, 1], ref allFitnesses[j, 1]);
                        Swap(ref allFitnesses[i, 0], ref allFitnesses[j, 0]);
                        break;
                    }

            int countNewIndividuals = (int)(individuals.Count * T);  //кол-во особей, прошедших через отбор
            List<int[]> newIndividuals = new List<int[]>();
            int numberNewIndividuals = 0;
            do  //отбор особей в новую популяцию
            {
                numberNewIndividuals = rand.Next(0, countNewIndividuals - 1);
                newIndividuals.Add(new int[countGenes]);
                for (int i = 0; i < countGenes; i++)
                    newIndividuals.Last()[i] = individuals[allFitnesses[numberNewIndividuals, 0]][i];
            } while (newIndividuals.Count < countIndividsSubpopulation);
            individuals.Clear();
            individuals = newIndividuals;
        }

        static void Inbreeding(List<int[]> individuals, int[,] allFitnesses, ref int[] parent_1, ref int[] parent_2)  //выбор родителей методом имбридинг
        {
            Random rand = new Random();
            int numberParent_1 = rand.Next(0, countIndividsSubpopulation - 1);  //выбор первого родителя          
            for (int i = 0; i < countGenes; i++)
                parent_1[i] = individuals[allFitnesses[numberParent_1, 0]][i];

            double[] euclideanDistance = new double[countIndividsSubpopulation];
            double minEuclideanDistance = double.MaxValue;
            int numberParent_2 = 0;
            for (int i = 0; i < countIndividsSubpopulation; i++)  //вычисление Евклидова расстояния для выбора второго родителя
            {
                for (int j = 0; j < countGenes; j++)
                    euclideanDistance[i] += Math.Pow(parent_1[j] - individuals[allFitnesses[i, 0]][j], 2);
                euclideanDistance[i] = Math.Sqrt(euclideanDistance[i]);
                if (euclideanDistance[i] < minEuclideanDistance && euclideanDistance[i] != 0)
                {
                    minEuclideanDistance = euclideanDistance[i];
                    numberParent_2 = i;
                }
            }
            for (int i = 0; i < countGenes; i++)
                parent_2[i] = individuals[allFitnesses[numberParent_2, 0]][i];
        }

        static void CrossingDiscreteRecombination(int[] parent_1, int[] parent_2, ref int[] descendant_1, ref int[] descendant_2)  //скрещивание методом дискретной рекомбинации
        {
            Random rand = new Random();
            int[,] maskForFiscreteRecombination = new int[2, countGenes];  //маска для замены генов
            for (int i = 0; i < 2; i++)    //выбираем номера особи для замены генов
                for (int j = 0; j < countGenes; j++)
                    maskForFiscreteRecombination[i, j] = rand.Next(0, 2);

            for (int i = 0; i < countGenes; i++)
            {
                if (maskForFiscreteRecombination[0, i] == 1)  //замена генов для первого потомка
                    descendant_1[i] = parent_2[i];
                else descendant_1[i] = parent_1[i];

                if (maskForFiscreteRecombination[1, i] == 0)   //замена генов для второго потомка
                    descendant_2[i] = parent_1[i];
                else descendant_2[i] = parent_2[i];
            }
        }

        static void MutationExchange(ref List<int[]> newIndividuals)  //мутация методом обмена соседних генов
        {
            Random rand = new Random();
            double T = 0.1;  //порог мутации
            double[] mutationProbability = new double[newIndividuals.Count];
            for (int i = 0; i < newIndividuals.Count; i++)
            {
                mutationProbability[i] = ((double)rand.Next(1, 100) / 100);  //случайно выбирается вероятность мутации для каждой особи
                if (mutationProbability[i] <= T)  //если вероятность мутации особи меньше порога
                {
                    int numForGeneMutations = rand.Next(0, countGenes - 1);  //номер гена для мутации    
                    newIndividuals[i][numForGeneMutations] = rand.Next(0, countGenes - 1);
                    //if (new_individuals[i][num_for_gene_mutations - 1] == new_individuals[i][num_for_gene_mutations + 1])
                    //    num_for_gene_mutations = rand.Next(0, count_genes - 1);
                    //if (num_for_gene_mutations == 0)
                    //    num_for_gene_mutations++;
                    //Swap(ref new_individuals[i][num_for_gene_mutations - 1], ref new_individuals[i][num_for_gene_mutations + 1]);
                }
            }
        }

        static void OutputPopulation(ref int[,] allFitnesses, int[,] newIndividuals)
        {
            int fitness = 0;
            for (int i = 0; i < countIndividsSubpopulation; i++)
            {
                fitness += matrWeidths[firstComp - 1, newIndividuals[i, 0]];
                Console.Write("Особь {0}, её гены: ", i + 1);
                for (int j = 0; j < countGenes; j++)
                {
                    if (newIndividuals[i, j] < 9)
                        Console.Write("{0},  ", newIndividuals[i, j] + 1);
                    else Console.Write("{0}, ", newIndividuals[i, j] + 1);
                    if (j > 0)
                        fitness += matrWeidths[newIndividuals[i, j - 1], newIndividuals[i, j]];
                }
                fitness += matrWeidths[newIndividuals[i, countGenes - 1], lastComp - 1];
                Console.WriteLine(" пригодность: " + fitness);
                allFitnesses[i, 0] = i;
                allFitnesses[i, 1] = fitness;
                fitness = 0;
            }
        }

        static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}
