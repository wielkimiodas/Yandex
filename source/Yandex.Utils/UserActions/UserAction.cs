using System;
using System.IO;

namespace Yandex.Utils.UserActions
{
    public abstract class UserAction
    {
        private static readonly char[] FieldsSep = new char[] {'\t'};

        /// <summary>
        /// Odczytuje dane z tablicy pól.
        /// </summary>
        /// <param name="array">Tablica z danymi odczytanymi z pliku logu.</param>
        /// <returns>True, jeżeli odczyt danych się powiedzie.</returns>
        public abstract bool ReadData(String[] array);

        /// <summary>
        /// Zapisuje dane do pliku.
        /// </summary>
        /// <returns>True, jeżeli operacja zapisu powiedzie się.</returns>
        public abstract bool WriteToStream(BinaryWriter writer);

        public static UserAction GetAction(String line)
        {
            char typeChar;
            String[] lineArray = line.Split(FieldsSep);
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

            if (!action.ReadData(lineArray))
                return null;

            return action;
        }
    }
}