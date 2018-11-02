﻿using System;
using System.IO;
using System.Windows.Forms;
using TVshowTracker.Classes;

namespace TVshowTracker
{
    public partial class MainForm : Form
    {
        // Checks if the Episodes are currently displayed on the screen, this is done to prevent Episodes from acting like Seasons and trying to access them.
        // Also used as a way to make shows with no season from trying to access No seasons message as a season and failing
        bool IsShowingEpisodes = false;
        bool HasSeasons = false;

        public MainForm()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MaximizeBox = false;
            ShowTVshows(); // loads show names to TVshowBox
        }

        /// <summary>
        /// Prints all TV shows to TV show Box
        /// </summary>
        private void ShowTVshows()
        {
            TVshowManager.SortShows(); // Sorts TV show list before writing to listbox
            foreach (var item in TVshowManager.TVshows)
            {
                TVShowBox.Items.Add(string.Format("{0}", item.TVname));
            }
        }

        /// <summary>
        /// Shows all seasons of a specific TV show name
        /// </summary>
        /// <param name="ShowName">TV show name</param>
        private void ShowSeasons(string ShowName)
        {
            int ShowIndex = FindShowIndex(ShowName);

            if (TVshowManager.TVshows[ShowIndex].Seasons.Count == 0)
            {
                EpisodeBox.Items.Add("No seasons available");
                IsShowingEpisodes = true;
                HasSeasons = false;
                return;
            }
            TVshowManager.TVshows[ShowIndex].SortSeasons();
            foreach (var item in TVshowManager.TVshows[ShowIndex].Seasons)
            {
                EpisodeBox.Items.Add(string.Format("Season {0}", item.SeasonNumber));
            }
            IsShowingEpisodes = false;
            HasSeasons = true;
        }

        /// <summary>
        /// Shows all episodes of a secified season
        /// </summary>
        /// <param name="SeasonIndex">seasons index</param>
        private void ShowEpisodes(int SeasonIndex)
        {
            int ShowIndex = FindShowIndex(TVShowBox.Items[TVShowBox.SelectedIndex].ToString());

            if (TVshowManager.TVshows[ShowIndex].Seasons[SeasonIndex].Episodes.Count == 0) // Checks if the season has any episodes in it
            {
                EpisodeBox.Items.Add("No episodes available");
                IsShowingEpisodes = true;
                return;
            }
            EpisodeBox.Items.Clear(); // Clears the EpisodeBox
            foreach (var item in TVshowManager.TVshows[ShowIndex].Seasons[SeasonIndex].Episodes)
            {
                EpisodeBox.Items.Add(string.Format("{0}", item.EpisodeName));
            }
            IsShowingEpisodes = true;
            
        }

        private void ShowINFO_Click(object sender, EventArgs e)
        {
            TVshowManager.SelectedShow = TVShowBox.SelectedIndex;
            var newform = new InfoForm();
            Hide();
            newform.ShowDialog();
            newform.Close();

            TVShowBox.Items.Clear(); // Clears listbox to write updated information
            ShowTVshows(); // Writes updates show list
            Show();
        }

        private void TVshows_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TVShowBox.SelectedIndex < 0)
            {
                return;
            }

            var index = TVShowBox.Items[TVShowBox.SelectedIndex].ToString();  
            EpisodeBox.Items.Clear();
            ShowSeasons(index);
        }

        private void AddShow_Click(object sender, EventArgs e)
        {
            var newform = new AddShowForm();
            Hide();
            newform.ShowDialog();
            newform.Close();
            
            TVShowBox.Items.Clear(); // Clears listbox to write updated information
            ShowTVshows(); // Writes updates show list
            Show();
        }

        private void AddEpisode_Click(object sender, EventArgs e)
        {
            if (IsShowingEpisodes && HasSeasons)
            {
                TVshowManager.SelectedShow = TVShowBox.SelectedIndex;
                var newform = new AddEpisodeForm();
                Hide();
                newform.ShowDialog();
                newform.Close();

                EpisodeBox.Items.Clear();
                ShowEpisodes(TVshowManager.SelectedSeason);
                Show();


            }
            else
            {
                AddSeason();
            }
           
        }

        private void AddSeason()
        {
            if (TVShowBox.SelectedIndex < 0)
            {
                return;
            }
            TVshowManager.TVshows[TVShowBox.SelectedIndex].Seasons.Add(new Season(TVshowManager.TVshows[TVShowBox.SelectedIndex].Seasons.Count + 1));
            EpisodeBox.Items.Clear();
            ShowSeasons(TVshowManager.TVshows[TVShowBox.SelectedIndex].TVname);
        }

        /// <summary>
        /// Rewrites the file according to TVshowManager TV show list
        /// </summary>
        /// <param name="TVIndex">TV show that is going to be rewritten</param>
        public static void RewriteFile(int TVIndex)
        {
            using (var writeFile = new StreamWriter(TVshowManager.TVshows[TVIndex].FilePath))
            {
                var obj = TVshowManager.TVshows[TVIndex];

                writeFile.WriteLine("{0};{1};{2};{3};", obj.TVname, obj.ReleaseDate, obj.EndingDate, obj.Tags);

                for (int i = 0; i < obj.Seasons.Count; i++)
                {
                    for (int j = 0; j < obj.Seasons[i].Episodes.Count; j++)
                    {
                        writeFile.WriteLine("{0};{1};{2};", obj.Seasons[i].SeasonNumber, obj.Seasons[i].Episodes[j].EpisodeName, obj.Seasons[i].Episodes[j].IsWatched);
                    }
                }
            }
        }

        private void RemoveShow_Click(object sender, EventArgs e)
        {
            if (TVShowBox.SelectedIndex >= 0)
            {
                File.Delete(TVshowManager.TVshows[TVShowBox.SelectedIndex].FilePath); // Deletes show's file in "TV shows" folder
                TVshowManager.TVshows.RemoveAt(TVShowBox.SelectedIndex); // Removes selected item in listbox from TvshowManager
                TVShowBox.Items.Clear(); // Clears listbox
                ShowTVshows(); // Writes updates show list
            }        
        }

        private void MarkedAsWatched_Click(object sender, EventArgs e)
        {

        }

        private void RemoveEpisode_Click(object sender, EventArgs e)
        {
            if (TVShowBox.SelectedIndex < 0 || TVshowManager.TVshows[TVShowBox.SelectedIndex].Seasons.Count == 0) // Check if there are any seasons to remove
            {
                return;
            }
            // Checks if the show has seasons, is NOT showing episodes 
            // Removes a season with all content
            else if (HasSeasons && !IsShowingEpisodes)
            {
                int i = TVshowManager.TVshows[TVShowBox.SelectedIndex].Seasons.Count - 1;
               
                TVshowManager.TVshows[TVShowBox.SelectedIndex].Seasons.RemoveAt(i); // Removes LAST season and all its content
                RewriteFile(TVShowBox.SelectedIndex);

                // Updates Episode box
                EpisodeBox.Items.Clear();
                ShowSeasons(TVshowManager.TVshows[TVShowBox.SelectedIndex].TVname);
            }
            // Removes episodes also REMOVES from file
            else if (IsShowingEpisodes && TVshowManager.TVshows[TVShowBox.SelectedIndex].Seasons[TVshowManager.SelectedSeason].Episodes.Count != 0)
            {
                var EpisodeIndex = TVshowManager.TVshows[TVShowBox.SelectedIndex].Seasons[TVshowManager.SelectedSeason].Episodes.Count - 1;
                TVshowManager.TVshows[TVShowBox.SelectedIndex].Seasons[TVshowManager.SelectedSeason].Episodes.RemoveAt(EpisodeIndex);
                RewriteFile(TVShowBox.SelectedIndex);

                EpisodeBox.Items.Clear();
                ShowEpisodes(TVshowManager.SelectedSeason);
            }       
            
        }

        private void EpisodeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = EpisodeBox.SelectedIndex;
            if (index < 0 || IsShowingEpisodes)
            {
                return;
            }
            else if (index >= 0 && !IsShowingEpisodes)
            {
                TVshowManager.SelectedSeason = EpisodeBox.SelectedIndex;
            }
            EpisodeBox.Items.Clear();
            ShowEpisodes(index);
          
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            TVShowBox.Items.Clear(); // Clears TVshow listbox

            foreach (var str in TVshowManager.TVshows)
            {
                if (str.TVname.ToLower().StartsWith(SearchBox.Text.ToLower()))
                {
                    TVShowBox.Items.Add(str.TVname); // Adds to the listbox only strings containing the string value in text box
                }
            }
        }

        private int FindShowIndex(string ShowName)
        {
            int ShowIndex = -1;
            for (int i = 0; i < TVshowManager.TVshows.Count; i++)
            {
                if (ShowName == TVshowManager.TVshows[i].TVname)
                {
                    ShowIndex = i;
                    break;
                }
            }
            return ShowIndex;
        }
    }
}
