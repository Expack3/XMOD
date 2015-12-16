using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace XACT_MOD_Player
{
    public class MODplayer : Microsoft.Xna.Framework.GameComponent
    {
        private int currLine; //current line in cache
        private int totalLines; //total number of lines in file
        public short playbackStatus = 0; //0 = stop, 1 = pause, 2 = play, 3 = loop
        private int lineGap; //used to determine, in milliseconds, the time between playing lines
        private int LPS; //How many LPS (lines per second) should be played?
        private String fileLocale;

        private Cue[] cueBank; //used to store all cues to be used for a song
        private String[] cueNames; //used to store the names of all cues to be used for a song
        private AudioEngine audioEngine; //pointer to the XACT audio engine
        private WaveBank waveBank; //pointer to the bank (collection) of audio samples used by audioEngine
        private SoundBank soundBank; //pointer to the bakn (collection) of XACT sounds used by audioEngine

        private Boolean componentInitialized;

        public int[,] soundCache; //used to store 6 seconds worth of songs based on the given LPS

        private MODloader v_loader; //used to store MODloader memory reference

        private int timer; //used to store how many milliseconds have gone by per timer count (currently between frames)

        public MODplayer(Game game)
            : base(game)
        {
            componentInitialized = false;
        }

        /// <summary>
        /// Initialize the player given the location of a XMOD file, and pointers to the XMOD loader, XACT audio engine, XACT wave bank, and XACT sound bank.
        /// </summary>
        /// <param name="fileLocation"></param>
        /// <param name="loader"></param>
        /// <param name="audioEngine"></param>
        /// <param name="waveBank"></param>
        /// <param name="soundBank"></param>
        public void initializeMODplayer(ref string fileLocation, ref MODloader loader, ref AudioEngine audioEngine, ref WaveBank waveBank, ref SoundBank soundBank)
        {
            if (componentInitialized == false)
            {
                //initialize variables
                v_loader = loader;
                this.audioEngine = audioEngine;
                this.waveBank = waveBank;
                this.soundBank = soundBank;
                cueBank = new Cue[4];
                fileLocale = fileLocation;


                v_loader.openFile(fileLocale); //load the file

                totalLines = v_loader.countLines();
                v_loader.readBeginningOfFile(out cueNames, out LPS);

                lineGap = 1000 / LPS; //divide 1000 by LPS (converted to a double)

                fullCacheLoad(); //completely load the song from the given file
                loader.closeFile();
                componentInitialized = true;
            }
        }

        /// <summary>
        /// Load the given XMOD file
        /// </summary>
        /// <param name="fileLocation"></param>
        public void loadNewFile(ref string fileLocation)
        {
            clearCaches(); //used to temporarily store the locations of the cues to be loaded into memory as sound effect instances.
            cueBank = new Cue[4];

            v_loader.openFile(fileLocation); //load the file

            totalLines = v_loader.countLines();
            v_loader.readBeginningOfFile(out cueNames, out LPS);

            lineGap = 1000 / LPS; //divide 1000 by LPS (converted to a double)

            fullCacheLoad(); //completely load the song from the given file
            v_loader.closeFile();
            componentInitialized = true;
        }

        private void clearCaches() //clears all the caches
        {
            DisposeAllSounds();
            cueBank = null;
            cueNames = null;
            fileLocale = null;

        }

        private void DisposeAllSounds() //disposes of any and all sounds currently stored in the cue bank.
        {
            foreach (Cue cue in cueBank)
            {
                if (cue != null)
                    cue.Dispose();
            }
        }

        /// <summary>
        /// Plays the currently-loaded song (if any)
        /// </summary>
        public void playXMOD()
        {
            playbackStatus = 2;
        }

        /// <summary>
        /// Pauses the currently-loaded song (if any)
        /// </summary>
        public void pauseXMOD()
        {
            playbackStatus = 1;
        }

        /// <summary>
        /// Stops the currently-loaded song (if any)
        /// </summary>
        public void stopXMOD()
        {
            playbackStatus = 0;
            currLine = 0;
            DisposeAllSounds();
        }

        /// <summary>
        /// Continuously plays the currently-loaded song (if any)
        /// </summary>
        public void loopXMOD()
        {
            playbackStatus = 3;
        }

        private void loopFile()
        {
            currLine = -1;
        }

        /// <summary>
        /// Returns the current line
        /// </summary>
        /// <returns></returns>
        public int getCurrentLine()
        {
            return currLine;
        }

        /// <summary>
        /// Sets the current line given a whole integer
        /// </summary>
        /// <param name="x"></param>
        public void setCurrentLine(int x)
        {
            currLine = x;
        }

        /// <summary>
        /// Returns an array of the song's instruments and their respective pitches
        /// </summary>
        /// <returns></returns>
        public int[] getInstrumentsAndPitch()
        {
            int[] tempString = new int[8];
            for (int x = 0; x < 8; x++)
            {
                tempString[x] = soundCache[currLine, x];
            }
            return tempString;
        }

        /// <summary>
        /// Return an array of a specific line from the song consisting of its instruments and their respective pitches
        /// </summary>
        /// <param name="desiredLine"></param>
        /// <returns></returns>
        public int[] getInstrumentsAndPitch(int desiredLine)
        {
            int[] tempString = new int[8];
            for (int x = 0; x < 8; x++)
            {
                tempString[x] = soundCache[desiredLine, x];
            }
            return tempString;
        }

        /// <summary>
        /// Convert a specific line in the song to text
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public String getLineInText(int x)
        {
            String tempString = String.Empty;
            for (int y = 0; y < 8; y++)
            {
                tempString = tempString + soundCache[x, y].ToString();
                if (y != 7)
                    tempString = tempString + ", ";
            }
            return tempString;
        }

        /// <summary>
        /// Gets the name of a specific cue
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public String getCueName(int x)
        {
            if (x < 0 || x + 1 > cueNames.Length)
            {
                switch (x)
                {
                    case -1:
                        return "null";
                    case -2:
                        return "EOF";
                    default:
                        return "null";
                }
            }
            else
            {
                if (cueNames[x] != null)
                    return cueNames[x];
                else
                    return "null";
            }
        }

        /// <summary>
        /// Allows for the modification and retreival of the current LPS (lines per second)
        /// </summary>
        public int lps
        {
            get
            {
                return LPS;
            }
            set
            {
                LPS = value;
                lineGap = 1000 / LPS; //update the interval between lines
            }
        }

        private void loadInstance(int x, int y, int z)//loads an instance of an instrument
        {
            if (!(cueNames[x].Equals(" ")))//does instrument name x not consist of a blank string?
            {
                if (cueBank[z] != null)
                    cueBank[z].Dispose();
                if (y < 10)//determine whether a 0 needs to be appended to the beginning of an instrument number
                    cueBank[z] = soundBank.GetCue(cueNames[x] + "_0" + y.ToString());
                else
                    cueBank[z] = soundBank.GetCue(cueNames[x] + "_" + y.ToString());

            }
        }

        /*private void initialCacheLoad() //depreciated cache loader; loaded six seconds-worth of music
        {
            soundCache = new int[(LPS * 6), 8];
            int[] tempLine;
            for (int x = 0; x < soundCache.GetLength(0); x++)
            {
                tempLine = v_loader.readLine();
                for (int y = 0; y < soundCache.GetLength(1); y++)
                {
                    if (tempLine != null)
                    {
                        soundCache[x, y] = tempLine[y];
                    }
                    else
                    {
                        soundCache[x, 0] = -2;
                        y = soundCache.GetLength(1);
                    }
                }
            }
        }*/

        private void fullCacheLoad()
        {
            soundCache = new int[totalLines, 10];
            int[] tempLine;
            for (int x = 0; x < soundCache.GetLength(0); x++)
            {
                tempLine = v_loader.readLine();
                for (int y = 0; y < soundCache.GetLength(1); y++)
                {
                    if (tempLine != null)
                    {
                        soundCache[x, y] = tempLine[y];
                    }
                    else
                    {
                        soundCache[x, 0] = -2;
                        y = soundCache.GetLength(1);
                    }
                }
            }
        }

        private void replaceLine()
        {
            int[] tempLine;
            tempLine = v_loader.readLine();
            for (int y = 0; y < soundCache.GetLength(1); y++)
            {
                if (tempLine != null)
                {
                    soundCache[currLine, y] = tempLine[y];
                }
                else
                {
                    soundCache[currLine, 0] = -2;
                    y = soundCache.GetLength(1);
                }
            }
        }

        public Boolean doesLineExist(int x)
        {
            if (x < 0 || x + 1 > soundCache.GetLength(0))
                return false;
            return true;
        }

        private void playLine()
        {
            if (currLine >= soundCache.GetLength(0)) //reset currLine to 0 if it exceeds the length of the first dimension of soundCache
                currLine = 0;
            for (int x = 0, y = 0; x < soundCache.GetLength(1); x = x + 2, y++)
            {
                switch (soundCache[currLine, x])
                {
                    case -3:
                        lps += soundCache[currLine, x + 1];
                        break;
                    case -2:
                        x = soundCache.GetLength(1); //force the for loop to exit out during the next iteration
                        if (playbackStatus != 3)
                        {
                            stopXMOD(); //tell the player to stop playback
                            currLine = -1;
                        }
                        else
                            loopFile();
                        return;
                    case -1:
                        break;
                    default:
                        if (cueBank[y] != null && cueBank[y].IsPlaying)
                        {
                            cueBank[y].Stop(AudioStopOptions.AsAuthored); //stop playing the song immediately if it's already playing
                        }
                        loadInstance(soundCache[currLine, x], soundCache[currLine, x + 1], y);
                        audioEngine.Update();
                        cueBank[y].Play(); //play x instrument on the current line
                        break;
                }
            }
            currLine++;
        }

        public void playLineStandalone()
        {
            if (currLine >= soundCache.GetLength(0)) //reset currLine to 0 if it exceeds the length of the first dimension of soundCache
                currLine = 0;
            for (int x = 0, y = 0; x < soundCache.GetLength(1); x = x + 2, y++)
            {
                switch (soundCache[currLine, 0])
                {
                    case -2:
                        x = soundCache.GetLength(1); //force the for loop to exit out during the next iteration
                        if (playbackStatus != 3)
                        {
                            stopXMOD(); //tell the player to stop playback
                            currLine = -1;
                        }
                        else
                            loopFile();
                        break;
                    case -1:
                        break;
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        if (cueBank[y] != null && cueBank[y].IsPlaying)
                        {
                            cueBank[y].Stop(AudioStopOptions.AsAuthored); //stop playing the song immediately if it's already playing
                        }
                        loadInstance(soundCache[currLine, x], soundCache[currLine, x + 1], y);
                        audioEngine.Update();
                        cueBank[y].Play(); //play x instrument on the current line
                        break;
                    case 4:
                        if (soundCache[currLine, x] != 0)
                        {
                            lps += soundCache[currLine, x];
                        }
                        break;
                }
            }
        }

        private void cleanString(ref String[] target)
        {
            for (int x = 0; x < target.Length; x++)
            {
                if (target[x].Length == 1 && Convert.ToChar(target[x]) == ' ')
                    target[x] = null;
            }
        }

        //Editor-specific code
        public void expandCache(int newSize)
        {
            if (newSize > 0 && newSize > soundCache.GetLength(0))
            {
                int[,] tempCache = new int[newSize, 9];
                for (int x = 0; x < newSize; x++)
                {
                    if (x < soundCache.GetLength(0))
                        tempCache = transferLine(tempCache, x);
                    else
                    {
                        for (int y = 0; y < 9; y++)
                        {
                            tempCache[x, y] = -1;
                        }
                    }
                }

                soundCache = tempCache;
            }
        }

        public void shrinkCache(int newSize)
        {
            if (newSize > 0 && newSize < soundCache.GetLength(0))
            {
                int[,] tempCache = new int[newSize, 9];
                for (int x = 0; x < newSize; x++)
                {
                    tempCache = transferLine(tempCache, x);
                }
                soundCache = tempCache;
            }
        }

        private int[,] transferLine(int[,] tempCache, int currentLine)
        {
            for (int x = 0; x < 9; x++)
            {
                tempCache[currentLine, x] = soundCache[currentLine, x];
            }
            return tempCache;
        }
        //End editor-specific code

        public override void Update(GameTime gameTime)
        {
            timer = timer + gameTime.ElapsedGameTime.Milliseconds;
            if (componentInitialized == true)
            {
                if (playbackStatus == 2 || playbackStatus == 3)
                {
                    if (timer >= lineGap)
                    {
                        playLine();
                        timer = 0;
                    }
                }
            }
            base.Update(gameTime);
        }

    }
}
