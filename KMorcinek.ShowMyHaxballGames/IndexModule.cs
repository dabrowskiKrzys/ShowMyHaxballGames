﻿using KMorcinek.ShowMyHaxballGames.Business;
using KMorcinek.ShowMyHaxballGames.Models;
using KMorcinek.ShowMyHaxballGames.ViewModelFactories;
using KMorcinek.ShowMyHaxballGames.ViewModels;
using Nancy;
using Nancy.Responses;
using System.Collections.Generic;
using System.Linq;

namespace KMorcinek.ShowMyHaxballGames
{

    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ =>
            {
                var db = DbRepository.GetDb();
                var leagues = db.UseOnceTo().Query<League>();

                var leagueViewModels = new List<LeagueViewModel>();

                foreach (var league in leagues.ToArray())
                {
                    leagueViewModels.Add(new LeagueViewModel(league));
                }

                var goodOrder = new LeaguesProvider().Get().Select(p => p.LeagueNumer).ToList();

                leagueViewModels = leagueViewModels.OrderBy(d => goodOrder.IndexOf(d.LeagueId)).ToList();

                return View["Index", leagueViewModels];
            };

            Get["/{leagueId:int}"] = _ =>
            {
                var leagueId = (int)_.leagueId.Value;

                var leagueViewModelFactory = new LeagueViewModelFactory();
                var leagueViewModel = leagueViewModelFactory.Create(leagueId);
                return View["League", leagueViewModel];
            };

            Get["/{leagueId:int}/{name}"] = _ =>
            {
                var leagueId = (int) _.leagueId.Value;
                var name = (string)_.name.Value;
                var gamesViewModelFactory = new GamesViewModelFactory();
                var gamesViewModel = gamesViewModelFactory.Create(leagueId, name);
                return View["Games", gamesViewModel];
            };
        }
    }
}