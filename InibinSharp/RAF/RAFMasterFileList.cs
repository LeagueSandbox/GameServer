#region LICENSE

// Copyright 2014 - 2014 InibinSharp
// RAFMasterFileList.cs is part of InibinSharp.
// InibinSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// InibinSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with InibinSharp. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace InibinSharp.RAF
{
    /// <summary>
    ///     Allows the easy manipulation of RAF archives. With this class the user can pretend there is only one giant RAF
    ///     archive
    /// </summary>
    public class RAFMasterFileList
    {
        private readonly Dictionary<String, List<RAFFileListEntry>> m_fileDictFull =
            new Dictionary<String, List<RAFFileListEntry>>();

        private readonly Dictionary<String, List<RAFFileListEntry>> m_fileDictShort =
            new Dictionary<String, List<RAFFileListEntry>>();

        /// <summary>
        ///     Allows the easy manipulation of RAF archives. With this class the user can pretend there is only one giant RAF
        ///     archive
        /// </summary>
        /// <param name="fileArchivePath">The path to RADS\projects\lol_game_client\filearchives</param>
        public RAFMasterFileList(String fileArchivePath)
        {
            var rafFilePaths = GetRAFFiles(fileArchivePath);

            foreach (var path in rafFilePaths)
            {
                var raf = new RAFArchive(path);

                m_fileDictFull = CombineFileDicts(m_fileDictFull, raf.FileDictFull);
                m_fileDictShort = CombineFileDicts(m_fileDictShort, raf.FileDictShort);
            }
        }

        /// <summary>
        ///     Allows the easy manipulation of RAF archives. With this class the user can pretend there is only one giant RAF
        ///     archive
        /// </summary>
        /// <param name="rafFilePaths">An array whose values are the paths to each RAF file you want to be combined together</param>
        public RAFMasterFileList(String[] rafFilePaths)
        {
            foreach (var path in rafFilePaths)
            {
                var raf = new RAFArchive(path);

                m_fileDictFull = CombineFileDicts(m_fileDictFull, raf.FileDictFull);
                m_fileDictShort = CombineFileDicts(m_fileDictShort, raf.FileDictShort);
            }
        }

        #region Accessors

        /// <summary>
        ///     Looks up the path in the RAFFileListEntry dictionary. The path must be exact. Use SearchFileEntries for partial
        ///     paths
        /// </summary>
        /// <param name="fullPath">Full RAFFileListEntry path, ie, DATA/Characters/Ahri/Ahri.skn (case insensitive)</param>
        /// <returns></returns>
        public List<RAFFileListEntry> GetFileEntry(string fullPath)
        {
            var lowerPath = fullPath.ToLower();
            List<RAFFileListEntry> returnValue;
            m_fileDictFull.TryGetValue(lowerPath, out returnValue);
            return returnValue;
        }

        /// <summary>
        ///     Returns the file dictionary which uses the full-path (lower-cased) file names as keys, ie.
        ///     "data/characters/ahri/ahri.skn"
        /// </summary>
        public Dictionary<String, List<RAFFileListEntry>> FileDictFull
        {
            get { return m_fileDictFull; }
        }

        /// <summary>
        ///     Returns the file dictionary which uses the (lower-cased) file names as keys, ie. "ahri.skn". The values are List
        ///     &lt;RAFFileListEntry&gt; to accomidate collisions
        /// </summary>
        public Dictionary<String, List<RAFFileListEntry>> FileDictShort
        {
            get { return m_fileDictShort; }
        }

        #endregion // Accessors

        #region Searching

        /// <summary>
        ///     Returns any entries whose filepath contains the search string. Use the RAFSearchType to specify how to search
        /// </summary>
        /// <param name="searchPhrase">The phrase to look for</param>
        /// <param name="searchType">
        ///     SearchType.All returns any entries whose filepath contains the search string. SearchType.End
        ///     returns any entries whose filepath ends with the search string.
        /// </param>
        /// <returns></returns>
        public List<RAFFileListEntry> SearchFileEntries(String searchPhrase,
            RAFSearchType searchType = RAFSearchType.All)
        {
            var lowerPhrase = searchPhrase.ToLower();
            var results = new List<RAFFileListEntry>();

            foreach (var entryKVP in m_fileDictFull)
            {
                foreach (var entry in entryKVP.Value)
                {
                    var lowerFilename = entry.FileName.ToLower();
                    if (searchType == RAFSearchType.All && lowerFilename.Contains(lowerPhrase))
                    {
                        results.Add(entry);
                    }
                    else if (searchType == RAFSearchType.End && lowerFilename.EndsWith(lowerPhrase))
                    {
                        results.Add(entry);
                    }
                }
            }
            return results;
        }

        /// <summary>
        ///     Simultaneously search for entries whose filepath contain a search phrase. Use the RAFSearchType to specify how to
        ///     search
        /// </summary>
        /// <param name="searchPhrases">Array of phrases to look for</param>
        /// <param name="searchType">
        ///     SearchType.All returns any entries whose filepath contains the search string. SearchType.End
        ///     returns any entries whose filepath ends with the search string.
        /// </param>
        /// <returns>A struct with the found RAFFileListEntry and the search phrase that triggered it</returns>
        public List<RAFSearchResult> SearchFileEntries(String[] searchPhrases,
            RAFSearchType searchType = RAFSearchType.All)
        {
            var results = new List<RAFSearchResult>();

            foreach (var entryKVP in m_fileDictFull)
            {
                foreach (var entry in entryKVP.Value)
                {
                    var lowerFilename = entry.FileName.ToLower();
                    foreach (var phrase in searchPhrases)
                    {
                        var lowerPhrase = phrase.ToLower();
                        if (searchType == RAFSearchType.All && lowerFilename.Contains(lowerPhrase))
                        {
                            RAFSearchResult result;
                            result.SearchPhrase = phrase;
                            result.Value = entry;
                            results.Add(result);
                            break;
                        }

                        if (searchType == RAFSearchType.End && lowerFilename.EndsWith(lowerPhrase))
                        {
                            RAFSearchResult result;
                            result.SearchPhrase = phrase;
                            result.Value = entry;
                            results.Add(result);
                            break;
                        }
                    }
                }
            }
            return results;
        }

        #endregion // Searching

        #region Helper functions

        /// <summary>
        ///     Searches each folder inside the base directory for .raf files, ignoring any sub-directories
        /// </summary>
        /// <param name="baseDir">The path to RADS\projects\lol_game_client\filearchives</param>
        /// <returns></returns>
        public List<String> GetRAFFiles(String baseDir)
        {
            var folders = Directory.GetDirectories(baseDir);

            var returnFiles = new List<String>();

            foreach (var folder in folders)
            {
                var files = Directory.GetFiles(folder, "*.raf", SearchOption.TopDirectoryOnly);
                //if (files.Length > 1)
                //throw new InvalidOperationException("Multiple RAF files found within specific archive folder.\nPlease delete your " + baseDir + "folder and repair your client");
                returnFiles.AddRange(files);
            }
            return returnFiles;
        }

        // Add Full dictionary to MasterFileList dict
        private static Dictionary<String, List<RAFFileListEntry>> CombineFileDicts(
            Dictionary<String, List<RAFFileListEntry>> dict1,
            Dictionary<String, RAFFileListEntry> dict2)
        {
            foreach (var entryKVP in dict2)
            {
                if (!dict1.ContainsKey(entryKVP.Key))
                    dict1.Add(entryKVP.Key, new List<RAFFileListEntry> {entryKVP.Value});
                else
                {
                    dict1[entryKVP.Key].Add(entryKVP.Value);
                }
            }
            return dict1;
        }

        // Add Short dictionary to MasterFileList dict
        private static Dictionary<String, List<RAFFileListEntry>> CombineFileDicts(
            Dictionary<String, List<RAFFileListEntry>> dict1,
            Dictionary<String, List<RAFFileListEntry>> dict2)
        {
            foreach (var entryKVP in dict2)
            {
                if (!dict1.ContainsKey(entryKVP.Key))
                    dict1.Add(entryKVP.Key, new List<RAFFileListEntry>(entryKVP.Value));
                else
                {
                    foreach (var entry in dict2[entryKVP.Key])
                    {
                        dict1[entryKVP.Key].Add(entry);
                    }
                }
            }
            return dict1;
        }

        #endregion // Helper functions
    }

    /// <summary>
    ///     Specifies how to do a phrase search
    /// </summary>
    public enum RAFSearchType
    {
        /// <summary>
        ///     Returns any entries whose filepath contains the search string, ie. "/ahri/" would return
        ///     DATA/Characters/Ahri/Ahri.skn
        /// </summary>
        All,

        /// <summary>
        ///     Returns any entries whose filepath ends with the search string, ie. "/ezreal_tx_cm.dds" would return
        ///     DATA/Characters/Ezreal/Ezreal_TX_CM.dds
        /// </summary>
        End
    }

    public struct RAFSearchResult
    {
        public String SearchPhrase;
        public RAFFileListEntry Value;
    }
}