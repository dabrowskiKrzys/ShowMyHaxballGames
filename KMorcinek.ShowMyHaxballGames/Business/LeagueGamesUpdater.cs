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

        public LeagueGamesUpdater(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _progressFactory = new ProgressFactory();
        }

        public void UpdateLeague(int leagueId, string title, List<Game> newGames, List<string> players)
        {
            var db = DbRepository.GetDb();
            var league = db.UseOnceTo().GetByQuery<League>(t => t.LeagueNumer == leagueId);

            if (league == null)
            {
                league = new League
                {
                    LeagueNumer = leagueId,
                    Title = title,
                    Players = players,
                    Games = new List<Game>(),
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
                
                league.Players = players;
                league.Progress = _progressFactory.Create(league);

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