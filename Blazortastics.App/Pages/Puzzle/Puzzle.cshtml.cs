using Blazortastics.App.Services;
using Blazortastics.App.Services.Puzzle;
using Blazortastics.Core.Base;
using Blazortastics.Core.Tools;
using Blazortastics.DB.Tables;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Blazortastics.App.Pages.Puzzle
{
    public class PuzzleModel : BindableBlazorComponent
    {
        #region cache
        private static string cache_playername;
        #endregion cache

        #region Enums
        private enum MoveDirection
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
            BottomToTop,
        }

        private enum GameState
        {
            Resetted = 0,
            Shuffling,
            Ready,
            Playing,
            Finished,
        }
        #endregion Enums

        [Inject] protected MessageBoxService MsgService { get; set; }
        [Inject] protected LeaderboardService LeaderboardService { get; set; }
        [Inject] protected IUriHelper UriHelper { get; set; }

        #region Variables
        private readonly Stopwatch watch = new Stopwatch();
        private GameState gameState = GameState.Resetted;
        #endregion Variables

        #region Properties

        public TimeSpan ElapsedTime { get; set; }
        public string ElapsedTimeString { get; set; }

        public long MoveCount { get; set; }

        public int SelectedRasterSize { get; set; } = 4;
        public int EffectiveRasterSize { get; set; } = 4; // diese Property ist live an die UI gebunden und beeinflusst das spielfeld
        public int? EffectiveShuffleGrade { get; set; }
        public int RasterFieldsCount { get; set; } = 16;
        public int[,] Raster { get; set; } = new int[4, 4];
        public int[] ItemPos { get; set; }

        [Parameter]
        protected string Playername { get; set; }
        #endregion Properties

        #region ctor
        public PuzzleModel()
        {

        }
        #endregion ctor

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (cache_playername != null && string.IsNullOrWhiteSpace(this.Playername))
            {
                this.Playername = cache_playername;
            }
        }

        protected override async Task OnInitAsync()
        {
            await this.ShuffleNewGameAsync(this.SelectedRasterSize);
        }

        private void ResetGame(int rasterSize)
        {
            this.MoveCount = 0L;

            this.watch.Reset();
            this.CaptureTimer();

            this.ResetField(rasterSize);

            this.gameState = GameState.Resetted;
        }

        #region GameTimer
        private void RestartTimer()
        {
            this.watch.Restart();

            Task.Run(async () =>
            {
                while (this.watch.IsRunning)
                {
                    this.CaptureTimer();
                    await Task.Delay(100);
                }
            });
        }

        private void StopTimer()
        {
            this.watch.Stop();
            this.CaptureTimer();
        }

        private void CaptureTimer()
        {
            this.ElapsedTime = this.watch.Elapsed;
            this.ElapsedTimeString = this.ElapsedTime.Format();

            this.StateHasChanged();
        }
        #endregion GameTimer

        #region RasterManipulation
        protected async Task ShuffleNewGameAsync(int rasterSize)
        {
            await this.ShuffleNewGameAsync(rasterSize, false);
        }

        protected async Task ShuffleNewGameAsync(int rasterSize, bool withAnimation)
        {
            if (this.gameState == GameState.Shuffling)
                return; // kein mehrfach-zeitgleich shuffling

            this.ResetGame(rasterSize);

            this.gameState = GameState.Shuffling;
            await this.ShuffleByRandomReverseMovesAsync(withAnimation);
            this.gameState = GameState.Ready;

            this.UpdateUIFromRaster();
        }

        protected async Task ShuffleNewGameAsync(int rasterSize, bool withAnimation, int reverseMovesCount)
        {
            if (this.gameState == GameState.Shuffling)
                return; // kein mehrfach-zeitgleich shuffling

            this.ResetGame(rasterSize);

            this.gameState = GameState.Shuffling;
            await this.ShuffleByRandomReverseMovesAsync(withAnimation, reverseMovesCount);
            this.gameState = GameState.Ready;

            this.UpdateUIFromRaster();
        }

        private async Task ShuffleByRandomReverseMovesAsync(bool withAnimation)
        {
            await this.ShuffleByRandomReverseMovesAsync(withAnimation, null);
        }

        private async Task ShuffleByRandomReverseMovesAsync(bool withAnimation, int? reverseMovesCount)
        {
            this.EffectiveShuffleGrade = reverseMovesCount;

            int validMovesToDoCount = reverseMovesCount ?? (int)(Math.Pow(this.EffectiveRasterSize, 3) + 200);

            if (reverseMovesCount == null)
            { // wird NULL angegeben, dann ist es ein 'normales' spiel
                for (int ring = this.EffectiveRasterSize; ring >= 2; ring -= 2)
                {
                    var roundtripsNeeded = (((ring - 1) * 2) - 1);
                    for (int roundtip = 1; roundtip <= roundtripsNeeded; roundtip++)
                    {
                        var (y, x) = this.GetPositionIndices(this.RasterFieldsCount); // position des blako-elements ermitteln
                        x = (x - (ring - 1));
                        this.MoveInternal(this.Raster[y, x]);
                        y = (y - (ring - 1));
                        this.MoveInternal(this.Raster[y, x]);
                        x = (x + (ring - 1));
                        this.MoveInternal(this.Raster[y, x]);
                        y = (y + (ring - 1));
                        this.MoveInternal(this.Raster[y, x]);
                    }

                    {
                        // move 1 up and 1 left to position in next ring
                        var (y, x) = this.GetPositionIndices(this.RasterFieldsCount); // position des blako-elements ermitteln
                        x--;
                        this.MoveInternal(this.Raster[y, x]);
                        y--;
                        this.MoveInternal(this.Raster[y, x]);
                    }
                }
            }

            var rnd = new Random();
            MoveDirection? lastDoneMoveDirection = null;
            int validMovesCount = 0;
            while (validMovesCount < validMovesToDoCount)
            {
                var id = rnd.Next(1, this.RasterFieldsCount); // which really means a range '1 <= x < 16' -> from inclusive 1 till exclusive 16

                if (this.CheckIsMoveValid(id) == false)
                { // kein gültiger zug, also zur laufzeitoptimierung per annäherung einen neuen berechnen
                    var coordinates = this.GetPositionIndices(id);
                    var blankoCoordinates = this.GetPositionIndices(this.RasterFieldsCount);

                    switch (lastDoneMoveDirection)
                    {
                        case MoveDirection.TopToBottom:
                        case MoveDirection.BottomToTop:
                            // wenn der vorherige zug eine vertikale bewegung hatte, dann nun einen horizontalen auslösen, also den wert der y-achse des blankos verwenden und die x-achse des zufälligen
                            id = this.Raster[blankoCoordinates.y, coordinates.x];
                            break;
                        case MoveDirection.LeftToRight:
                        case MoveDirection.RightToLeft:
                            // wenn der vorherige zug eine horizontale bewegung hatte, dann nun einen vertikalen auslösen, also den wert der y-achse des zufälligen verwenden und die x-achse des blankos
                            id = this.Raster[coordinates.y, blankoCoordinates.x];
                            break;
                    }
                }

                var thisMoveDirection = this.CheckPotentialMoveDirection(id);

                if ((lastDoneMoveDirection == MoveDirection.TopToBottom || lastDoneMoveDirection == MoveDirection.BottomToTop) && (thisMoveDirection == MoveDirection.TopToBottom || thisMoveDirection == MoveDirection.BottomToTop))
                    continue; // potenziell gleiche axenbewegung -> unterbinden, damit besser gemischt wird
                else if ((lastDoneMoveDirection == MoveDirection.LeftToRight || lastDoneMoveDirection == MoveDirection.RightToLeft) && (thisMoveDirection == MoveDirection.LeftToRight || thisMoveDirection == MoveDirection.RightToLeft))
                    continue; // potenziell gleiche axenbewegung -> unterbinden, damit besser gemischt wird

                if (this.MoveInternal(id))
                {
                    validMovesCount++;
                    lastDoneMoveDirection = thisMoveDirection;
                    if (withAnimation)
                    {
                        this.UpdateUIFromRaster();
                        await Task.Delay(10);
                    }
                }
            }
        }

        private void ResetField(int rasterSize)
        {
            this.EffectiveRasterSize = rasterSize;
            this.RasterFieldsCount = (int)Math.Pow(rasterSize, 2);
            this.ItemPos = Enumerable.Range(1, this.RasterFieldsCount).ToArray();

            this.Raster = new int[rasterSize, rasterSize];

            for (int y = 0; y <= rasterSize - 1; y++)
            {
                for (int x = 0; x <= rasterSize - 1; x++)
                {
                    this.Raster[y, x] = (y * rasterSize) + (x + 1);
                }
            }

            this.UpdateUIFromRaster();
        }

        private void UpdateUIFromRaster()
        {
            for (int id = 1; id <= this.RasterFieldsCount; id++)
            {
                this.ItemPos[id - 1] = this.GetPosition(this.GetPositionIndices(id));
            }

            this.StateHasChanged();
        }
        #endregion RasterManipulation

        #region PositionCalculation
        private int GetPosition((int y, int x) indices)
        {
            return (((indices.y) * this.EffectiveRasterSize) + (indices.x + 1));
        }

        private (int y, int x) GetPositionIndices(int id)
        {
            for (int i = 0; i < this.EffectiveRasterSize; i++)
            {
                for (int j = 0; j < this.EffectiveRasterSize; j++)
                {
                    if (this.Raster[i, j] == id)
                    {
                        return (i, j);
                    }
                }
            }

            return (-1, -1);
        }
        #endregion PositionCalculation

        #region Move
        private MoveDirection? CheckPotentialMoveDirection(int id)
        {
            if (this.CheckIsMoveValid(id) == false)
                return null;

            var (y, x) = this.GetPositionIndices(id);
            var blankIndices = this.GetPositionIndices(this.RasterFieldsCount);

            if (y == blankIndices.y && x < blankIndices.x)
            { // clicked left of blank
                return MoveDirection.RightToLeft;
            }
            else if (y == blankIndices.y && x > blankIndices.x)
            { // clicked right of blank
                return MoveDirection.LeftToRight;
            }
            else if (y < blankIndices.y && x == blankIndices.x)
            { // clicked over of blank
                return MoveDirection.BottomToTop;
            }
            else if (y > blankIndices.y && x == blankIndices.x)
            { // clicked under of blank
                return MoveDirection.TopToBottom;
            }

            return MoveDirection.RightToLeft; // todo: anpassen!!!
        }

        /// <summary>
        /// Tries to move the blocks depending on where the user clicked.
        /// </summary>
        /// <param name="id">The Id of the block the user clicked on.</param>
        protected async Task<bool> MoveAsync(int id)
        {
            if (this.gameState != GameState.Ready && this.gameState != GameState.Playing)
                return false; // manuelle Züge nur gültig, wenn state auf 'ready' oder 'playing'

            if (this.CheckIsMoveValid(id) == false)
                return false;

            if (this.gameState == GameState.Ready)
            { // wenn der timer noch nicht läuft, dann beim ersten tatsächlichen Move starten
                this.gameState = GameState.Playing;
                this.RestartTimer();
            }

            // raster-internen move ausführen
            this.MoveInternal(id);
            this.MoveCount++;

            // UI updaten
            this.UpdateUIFromRaster();

            // Gewinnbedingung prüfen
            await this.CheckWinAsync();

            return true;
        }


        /// <summary>
        /// Tätigt den angegebenen Move.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>the moves validity</returns>
        private bool MoveInternal(int id)
        {
            if (this.CheckIsMoveValid(id) == false)
                return false;

            var (y, x) = this.GetPositionIndices(id);
            var blankIndices = this.GetPositionIndices(this.RasterFieldsCount);

            if (y == blankIndices.y && x < blankIndices.x)
            { // clicked left of blank
                this.Move(MoveDirection.RightToLeft, blankIndices.x, x, blankIndices.y);
            }
            else if (y == blankIndices.y && x > blankIndices.x)
            { // clicked right of blank
                this.Move(MoveDirection.LeftToRight, blankIndices.x, x, blankIndices.y);
            }
            else if (y < blankIndices.y && x == blankIndices.x)
            { // clicked over of blank
                this.Move(MoveDirection.BottomToTop, blankIndices.y, y, blankIndices.x);
            }
            else if (y > blankIndices.y && x == blankIndices.x)
            { // clicked under of blank
                this.Move(MoveDirection.TopToBottom, blankIndices.y, y, blankIndices.x);
            }

            return true;
        }

        /// <summary>
        /// Prüft, ob ein Zug gültig wäre.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>the moves validity</returns>
        private bool CheckIsMoveValid(int id)
        {
            if (id == this.RasterFieldsCount)
                return false; // blanko geklickt -> nichts tun

            var (y, x) = this.GetPositionIndices(id);
            var blankIndices = this.GetPositionIndices(this.RasterFieldsCount);

            if (y != blankIndices.y && x != blankIndices.x)
                return false; // weder gleiche Zeile, noch gleiche Spalte wie blanko geklickt -> nichts tun

            return true;
        }

        private void Move(MoveDirection direction, int start, int end, int staticAxe)
        {
            switch (direction)
            {
                case MoveDirection.LeftToRight:
                    for (int current = start; current <= end; current++)
                    {
                        if (current == end)
                        { // am ziel angekommen -> blank setzen
                            this.Raster[staticAxe, current] = this.RasterFieldsCount;
                        }
                        else
                        { // shift one to the right
                            this.Raster[staticAxe, current] = this.Raster[staticAxe, current + 1];
                        }
                    }
                    break;
                case MoveDirection.RightToLeft:
                    for (int current = start; current >= end; current--)
                    {
                        if (current == end)
                        { // am ziel angekommen -> blank setzen
                            this.Raster[staticAxe, current] = this.RasterFieldsCount;
                        }
                        else
                        { // shift one to the left
                            this.Raster[staticAxe, current] = this.Raster[staticAxe, current - 1];
                        }
                    }
                    break;
                case MoveDirection.TopToBottom:
                    for (int current = start; current <= end; current++)
                    {
                        if (current == end)
                        { // am ziel angekommen -> blank setzen
                            this.Raster[current, staticAxe] = this.RasterFieldsCount;
                        }
                        else
                        { // shift one to the bottom
                            this.Raster[current, staticAxe] = this.Raster[current + 1, staticAxe];
                        }
                    }
                    break;
                case MoveDirection.BottomToTop:
                    for (int current = start; current >= end; current--)
                    {
                        if (current == end)
                        { // am ziel angekommen -> blank setzen
                            this.Raster[current, staticAxe] = this.RasterFieldsCount;
                        }
                        else
                        { // shift one to the top
                            this.Raster[current, staticAxe] = this.Raster[current - 1, staticAxe];
                        }
                    }
                    break;
            }
        }
        #endregion Move

        #region WinCondition
        private async Task<bool> CheckWinAsync()
        {
            if (this.GetHasWon())
            {
                this.gameState = GameState.Finished;
                this.StopTimer();

                await this.InsertRankingEntryAsync();

                this.ShowDelayedMessageAsync($"You have got it '{this.Playername}'!!!{Environment.NewLine}{Environment.NewLine}time: {this.ElapsedTimeString}{Environment.NewLine}moves: {this.MoveCount}", 500);

                return true;
            }

            return false;
        }

        private async Task InsertRankingEntryAsync()
        {
            if (this.gameState == GameState.Finished)
            {
                var rankingEntry = new Ranking();
                rankingEntry.Fieldsize = this.EffectiveRasterSize;
                rankingEntry.ShuffleGrade = this.EffectiveShuffleGrade;
                rankingEntry.Name = (string.IsNullOrWhiteSpace(this.Playername) ? "anon" : this.Playername);
                rankingEntry.Time = this.ElapsedTime;
                rankingEntry.Moves = this.MoveCount;
                rankingEntry.Timestamp = DateTime.Now;
                rankingEntry.Comment = string.Empty;

                await this.LeaderboardService.InsertRankingAsync(rankingEntry);
            }
        }


        private bool GetHasWon()
        {
            var idsByPos = new List<decimal>();

            for (int y = 0; y <= this.EffectiveRasterSize - 1; y++)
            {
                for (int x = 0; x <= this.EffectiveRasterSize - 1; x++)
                {
                    idsByPos.Add(this.Raster[y, x]);
                }
            }

            // normale gewinnbedingung mit dem blanko feld rechts unten
            var result = idsByPos.Aggregate((prev, cur) => prev < cur ? cur : decimal.MaxValue);
            if (result != decimal.MaxValue)
            { // dann sollten alle werte in einer aufsteigenden reihenfolge sein
                return true;
            }

            // manchmal ist dies nicht möglich, dann muss das blankofeld rechts oben plazert werden
            // also für die prüfung die <id des blankoelements> durch einen wert ersetzen, der zwischen dem letzen element der ersten zeile und dem nachfolgenden, bereits in die zweite zweile geschobenen element liegt (bei rastergröße 4 wäre das z.B. 3.5)
            idsByPos[idsByPos.IndexOf(this.RasterFieldsCount)] = (this.EffectiveRasterSize - 0.5M);
            result = idsByPos.Aggregate((prev, cur) => prev < cur ? cur : decimal.MaxValue);
            if (result != decimal.MaxValue)
            { // dann sollten alle werte in einer aufsteigenden reihenfolge sein
                return true;
            }

            return false;
        }
        #endregion WinCondition

        #region Helpers
        private async void ShowDelayedMessageAsync(string message, int delay)
        {
            await Task.Delay(delay);
            await this.MsgService.Alert(message);
        }
        #endregion Helpers

        protected void ShowLeaderboard(int fieldsize, string playername, int? shuffleGrade)
        {
            if (string.IsNullOrWhiteSpace(playername) == false)
            {
                if (shuffleGrade != null)
                {
                    this.UriHelper.NavigateTo($"/puzzle_Leaderboard/fs/{fieldsize}/n/{playername}/s/{shuffleGrade}");
                }
                else
                {
                    this.UriHelper.NavigateTo($"/puzzle_Leaderboard/fs/{fieldsize}/n/{playername}");
                }
            }
            else
            {
                if (shuffleGrade != null)
                {
                    this.UriHelper.NavigateTo($"/puzzle_Leaderboard/fs/{fieldsize}/s/{shuffleGrade}");
                }
                else
                {
                    this.UriHelper.NavigateTo($"/puzzle_Leaderboard/fs/{fieldsize}");
                }
            }
        }

        protected override void DisposeManagedCode()
        {
            base.DisposeManagedCode();

            cache_playername = this.Playername;
        }
    }
}