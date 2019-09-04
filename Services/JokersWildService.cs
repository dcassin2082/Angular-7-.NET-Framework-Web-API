﻿using System.Collections.Generic;
using System.Linq;
using WebApi.Helpers;
using WebApi.Models;

namespace WebApi.Services
{
    public class JokersWildService : ServicesBase, IJokersWildService
    {
        private static List<Card> _dealCards = new List<Card>();
        private static List<Card> _drawCards = new List<Card>();

        public GameModel Deal(int bet, string username)
        {
            UserAccount userAccount = dbContext.UserAccounts.Where(u => u.Username == username).FirstOrDefault();
            List<int> shuffle = GameAction.ShuffleDeckWithJokers();
            _dealCards = new List<Card>();
            foreach (var item in shuffle)
            {
                var card = dbContext.Cards.Where(c => c.CardId == item).FirstOrDefault();
                _dealCards.Add(card);
            }
            GameModel model = new GameModel
            {
                Message = ApplicationConstants.HoldDrawMessage,
                Hand = _dealCards.Take(5).ToList(),
                GameOver = false,
                BetAmount = bet,
                JokersWildPaytable = dbContext.JokersWildPayouts.ToList(),
                Credits = (int)userAccount.Credits - bet
            };
            JokersWildHandInfo.EvaluateHand(model, _dealCards, userAccount);
            if (model.Credits <= 0)
            {
                model.Credits = 100;
                userAccount.Credits = 100;
                dbContext.SaveChanges();
            }
            model.JokersWildPaytable = model.JokersWildPaytable.OrderByDescending(e => e.Id).ToList();
            return model;
        }

        public GameModel Draw(int[] arr, int bet, string username)
        {
            _drawCards = new List<Card>();
            UserAccount userAccount = dbContext.UserAccounts.Where(u => u.Username == username).FirstOrDefault();
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == 0)
                {
                    _drawCards.Add(_dealCards[i + 5]);
                }
                else
                {
                    _drawCards.Add(_dealCards[i]);
                }
            }
            GameModel model = new GameModel
            {
                Message = ApplicationConstants.GameOver,
                Hand = _drawCards,
                GameOver = true,
                Credits = (int)userAccount.Credits,
                BetAmount = bet,
                JokersWildPaytable = dbContext.JokersWildPayouts.ToList()
            };
            JokersWildHandInfo.EvaluateHand(model, _drawCards, userAccount);
            if (model.Credits <= 0)
            {
                model.Credits = 100;
                userAccount.Credits = 100;
                dbContext.SaveChanges();
            }
            return model;
        }

        public GameModel GetPaytable(string username)
        {
            UserAccount userAccount = dbContext.UserAccounts.Where(u => u.Username == username).FirstOrDefault();
            GameModel model = new GameModel
            {
                Message = ApplicationConstants.GoodLuck,
                GameOver = false,
                JokersWildPaytable = dbContext.JokersWildPayouts.OrderByDescending(e => e.Id).ToList(),
                Credits = (int)userAccount.Credits
            };
            if (model.Credits <= 0)
            {
                model.Credits = 100;
                userAccount.Credits = 100;
                dbContext.SaveChanges();
            }
            return model;
        }
    }
}