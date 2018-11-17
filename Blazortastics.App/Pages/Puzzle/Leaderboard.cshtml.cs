using Blazortastics.App.Services;
using Blazortastics.App.Services.Puzzle;
using Blazortastics.Core.Base;
using Blazortastics.Core.Tools;
using Blazortastics.DB.Tables;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazortastics.App.Pages.Puzzle
{
    public class PuzzleLeaderboardModel : BindableBlazorComponent
    {
        #region Inject
        [Inject] protected MessageBoxService MessageBoxService { get; set; }
        [Inject] protected LeaderboardService LeaderboardService { get; set; }
        [Inject] protected IUriHelper UriHelper { get; set; }
        #endregion Inject

        #region Parameters
        [Parameter]
        protected string FieldSize { get; set; }

        private int selectedFieldSize;
        public int SelectedFieldSize
        {
            get { return selectedFieldSize; }
            set { this.SetProperty(ref selectedFieldSize, value); }
        }

        [Parameter]
        protected string Playername { get; set; }

        [Parameter]
        protected string ShuffleGrade { get; set; }
        #endregion Parameters

        #region Properties
        public List<Ranking> AllRankings { get; set; } = new List<Ranking>();
        public List<Ranking> DisplayedRankings { get; set; } = new List<Ranking>();

        private int selectedShuffleGrade = -1; // da nullable nicht geht (derzeit?) wird hier -1 als sonderwert für null verwendet
        public int SelectedShuffleGrade
        {
            get { return selectedShuffleGrade; }
            set { this.SetProperty(ref selectedShuffleGrade, value); }
        }

        private DateTime selectedMonth = DateTime.Today;
        public DateTime SelectedMonth
        {
            get { return selectedMonth; }
            set { this.SetProperty(ref selectedMonth, value); }
        }

        #endregion Properties

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (this.FieldSize == default)
            {
                this.FieldSize = "4";
            }
            this.selectedFieldSize = Convert.ToInt32(this.FieldSize); // absichtlich ohne PropertyChanged


            if (this.ShuffleGrade == default)
            {
                this.ShuffleGrade = "-1";
            }
            this.selectedShuffleGrade = Convert.ToInt32(this.ShuffleGrade); // absichtlich ohne PropertyChanged
        }

        protected override async Task OnInitAsync()
        {
            await this.RefreshRemoteAsync();
        }

        protected async Task RefreshRemoteAsync()
        {
            this.AllRankings = (await this.LeaderboardService.GetRankingsAsync(this.SelectedMonth)).ToList();

            this.RefreshLocal();
        }

        protected void RefreshLocal()
        {
            int? sg = (this.SelectedShuffleGrade == -1 ? null : (int?)this.SelectedShuffleGrade);

            var filteredRankings = this.AllRankings
                .Where(a => a.Fieldsize == this.SelectedFieldSize && a.ShuffleGrade == sg
                    && a.Timestamp.Year == DateTime.Today.Year && a.Timestamp.Month == DateTime.Today.Month);

            var sortedRankings = filteredRankings
                .OrderBy(a => a.Time).ThenBy(a => a.Moves)
                .ToList();

            foreach (var ranking in sortedRankings)
            {
                ranking.Position = sortedRankings.IndexOf(ranking) + 1;
                ranking.FormattedTime = ranking.Time.Format();
            }

            this.DisplayedRankings = sortedRankings.ToList();

            this.StateHasChanged();
        }

        protected void NavigateBackToGame()
        {
            if (string.IsNullOrWhiteSpace(this.Playername))
            {
                this.UriHelper.NavigateTo($"/puzzle/");
            }
            else
            {
                this.UriHelper.NavigateTo($"/puzzle/n/{this.Playername}");
            }
        }

        protected override async void OnPropertyChanged<T>(string propertyName, T oldValue, T newValue)
        {
            switch (propertyName)
            {
                case nameof(this.SelectedFieldSize):
                    this.RefreshLocal();
                    break;
                case nameof(this.SelectedShuffleGrade):
                    this.RefreshLocal();
                    break;
                case nameof(this.SelectedMonth):
                    await this.RefreshRemoteAsync();
                    break;
                default:
                    break;
            }
        }
    }
}