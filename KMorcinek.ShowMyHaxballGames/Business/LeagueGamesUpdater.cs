﻿using System.Collections.Generic;
using System.Linq;
using KMorcinek.ShowMyHaxballGames.Factories;
using KMorcinek.ShowMyHaxballGames.Models;
using KMorcinek.ShowMyHaxballGames.Utils;

namespace KMorcinek.ShowMyHaxballGames.Business
{
    public class LeagueGamesUpdater
    {
        private readonly ITimeProvider _timeProvider;
        private readonly ProgressFactory _progressFactory;

        public LeagueGamesUpdater(ITimeProvider timeProvider, ProgressFactory progressFactory)
        {
            _timeProvider = timeProvider;
            _progressFactory = progressFactory;
        }

        public void UpdateLeague(int leagueId, string title, List<Game> newGames, List<string> players, int seasonNumber, string winner)
        {
            var db = DbRepository.GetDb();
            var league = db.UseOnceTo().GetByQuery<League>(t => t.LeagueNumer == leagueId);

            if (league == null)
            {
                league = new League
                {
                    LeagueNumer = leagueId,
                    SeasonNumber = seasonNumber,
                    Title = title,
                    Players = players,
                    Games = new List<Game>(),
                    Winner = winner,
                };

                foreach (var newGame in newGames)
                {
                    var gameCopy = newGame.GetDeepCopy();

                    if (newGame.Result != Constants.NotPlayed)
                        gameCopy.PlayedDate = _timeProvider.GetCurrentTime();

                    league.Games.Add(gameCopy);
                }

                league.Progress = _progressFactory.Create(league);

                db.UseOnceTo().Insert(league);
            }
            else
            {
                UpdateLeague(league, newGames);

                league.SeasonNumber = seasonNumber;
                league.Players = players;
                league.Progress = _progressFactory.Create(league);
                league.Winner = winner;

                db.UseOnceTo().Update(league);
            }
        }

        public void UpdateLeague(League league, List<Game> newGames)
        {
            var notPlayedOldGames = league.Games.Where(g => g.Result == Constants.NotPlayed);

            foreach (var oldGame in notPlayedOldGames)
            {
                var matchingNewGame =
                    newGames.SingleOrDefault(
                        g => g.HomePlayer == oldGame.HomePlayer && g.AwayPlayer == oldGame.AwayPlayer);

                if (matchingNewGame != null && matchingNewGame.Result != Constants.NotPlayed)
                {
                    oldGame.Result = matchingNewGame.Result;
                    oldGame.PlayedDate = _timeProvider.GetCurrentTime();
                }
            }
        }
    }
}