using System;

namespace Yandex.Transformer
{
    public abstract class UserAction
    {
        private static char[] fieldsSep = new char[] {'\t'};

        /// <summary>
        /// Odczytuje dane z tablicy pól.
        /// </summary>
        /// <param name="array">Tablica z danymi odczytanymi z pliku logu.</param>
        /// <returns>True, jeżeli odczyt danych się powiedzie.</returns>
        public abstract bool readData(String[] array);

        /// <summary>
        /// Zapisuje dane do pliku.
        /// </summary>
        /// <returns>True, jeżeli operacja zapisu powiedzie się.</returns>
        public abstract bool writeToFile(BufferedBinaryWriter writer);

        public static UserAction getAction(String line)
        {
            String[] array = line.Split(fieldsSep);
            char typeChar = 'A';
            String[] lineArray = line.Split(fieldsSep);
            if (Char.IsLetter(lineArray[1][0]))
                typeChar = lineArray[1][0];
            else if (Char.IsLetter(lineArray[2][0]))
                typeChar = lineArray[2][0];
            else
                return null;

            UserAction action = null;

            switch (typeChar)
            {
                case 'M':
                    action = new Metadata();
                    break;
                case 'Q':
                case 'T':
                    action = new QueryAction();
                    break;
                case 'C':
                    action = new Click();
                    break;
                default:
                    return null;
            }

            if (!action.readData(array))
                return null;

            return action;
        }
    }
}