using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace XACT_MOD_Player
{
    public class MODloader
    {
        StreamReader reader; //used to read the XMOD sound module
        String line = String.Empty; //used to store a text line for processing
        String fileName = String.Empty; //used to cache name of file
        Boolean beginningRead = false; //has the beginning of the MOD file been read?
        public void openFile(String filename)
        {
            fileName = filename;
            if (File.Exists(filename))
            {
#if DEBUG && WINDOWS
                Console.WriteLine("Opening script...");
#endif
                reader = new StreamReader(filename);
            }
            else
            {
#if DEBUG && WINDOWS
                Console.WriteLine("Could not open file. File does not exist.");
#endif
            }

        }

        public void reloadFile()
        {
            if (fileName != null)
            {
                if (File.Exists(fileName))
                {
#if DEBUG && WINDOWS
                    Console.WriteLine("Opening script...");
#endif
                    reader = new StreamReader(fileName);
                    beginningRead = false;
                }
                else
                {
#if DEBUG && WINDOWS
                    Console.WriteLine("Could not open file. File does not exist.");
#endif
                }
            }
        }

        public void readBeginningOfFile(out string[] fileList, out int LPS)
        {
            if (beginningRead == false) //check to see if the beginning of the file has been read
            {
                line = reader.ReadLine();
                fileList = line.Split(','); //Load the first line
                line = reader.ReadLine();
                LPS = Convert.ToInt16(line); //con
                beginningRead = true;
            }
            else
            {
                fileList = null;
                LPS = 0;
            }
        }

        public void fastReadBeginning() //advance the file past the first two lines
        {
            if (beginningRead == false) //check to see if the beginning of the file has been read
            {
                reader.ReadLine();
                reader.ReadLine();
                beginningRead = true;
            }
        }



        public int[] readLine()
        {
            if (!reader.EndOfStream) //has EOF been reached?
            {
                line = reader.ReadLine();
                string[] splitLine = line.Split(',');
                int[] finalLine = convertLineToInt(ref splitLine);
                return finalLine;
            }
            else
            {
                //otherwise, set the first position of the current cache line to -2, the EOF indicator for the XMOD file format
                return null;
            }
        }

        private int[] convertLineToInt(ref string[] line)
        {
            int[] outLine = new int[10];
            for (int x = 0; x < outLine.Length; x++)
            {
                outLine[x] = Convert.ToInt16(line[x]);
            }
            return outLine;
        }

        public void closeFile()
        {
            reader.Close();
            beginningRead = false;
            fileName = String.Empty;
        }

        public int countLines()
        {
            int currMODline = 1;
            reloadFile();
            beginningRead = false;
            fastReadBeginning();
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                currMODline++;
            }
            reloadFile();
            return currMODline;
        }

        private void cleanLine(ref string[] line) //if the first line of the file is greater than 7 elements (XACT audio file elements + 4 sounds), remove additional elements and trim
        {
            if (line.Length > 9)
            {
                for (int x = 9; x < line.Length; x++)
                {
                    line[x] = null;
                    line[x].Trim();
                }
            }
        }
    }
}
