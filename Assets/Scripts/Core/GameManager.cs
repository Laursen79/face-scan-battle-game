using System;
using System.Collections;
using System.Collections.Generic;
using Battle.Laursen;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Battle
{
    public class GameManager : Singleton<GameManager>
    {
        public UnityEvent startRound;


        private IEnumerator StartRound(int countdown)
        {
            yield return new WaitForSeconds(countdown);
            startRound.Invoke();
        }

        private void Awake()
        {
            
        }

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public static void Die(int player)
        {
            SceneManager.LoadScene(player);
        }
        
        PlayerToken CreatePlayerToken(string str)
        {
            var encoding = new System.Text.ASCIIEncoding ();
            var bytes = encoding.GetBytes (str);
            var sha = new System.Security.Cryptography.SHA1CryptoServiceProvider ();
            var hash = BitConverter.ToString(sha.ComputeHash(bytes));
            return new PlayerToken(hash);
        }

        void SaveScore(PlayerToken token)
        {
            PlayerPrefs.SetInt(token.id, token.score);
            PlayerPrefs.Save();
        }
        void SaveScore(string id, int score)
        {
            PlayerPrefs.SetInt(id, score);
            PlayerPrefs.Save();
        }
        
        ///USE WITH CAUTION. Clears All scores.
        void ResetAllScores() 
        {
            PlayerPrefs.DeleteAll();
        }
    }
    [Serializable]
    public struct PlayerToken
    {
        public string id;
        public int score;
    
        public PlayerToken(string id)
        {
            this.id = id;
            score = 0;
        }
    }
}