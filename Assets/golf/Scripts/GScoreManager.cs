using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//an enum to handle all the possible scoring events
public enum eGScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}
//ScoreManager handles all of the scoring
public class GScoreManager : MonoBehaviour
{
    static private GScoreManager S;
    //static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Set Dynamically")]
    //fields to track score info
    //public int chain = 0;
    //public int scoreRun = 0;
    public int score = 0;
    public List<CardGolfSolitaire> tableau;
    void Awake()
    {
        if (S == null)
        {
            S = this; //set the private singleton
        }
        else
        {
            Debug.LogError("ERROR: GScoreManager.Awake(): S is already set!");
        }
        //check for a high score in PlayerPrefs
        if (PlayerPrefs.HasKey("GolfSolitaireHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("GolfSolitaireHighScore");
        }
        //add the score from last round, which will be >0 if it was a win
        //score += SCORE_FROM_PREV_ROUND;
        //and reset the SCORE_FROM_PREV_ROUND
        //SCORE_FROM_PREV_ROUND = 0;
    }
    static public void EVENT(eGScoreEvent evt)
    {
        try
        {
            //try-catch stops an error from breaking your program
            S.Event(evt);
        }
        catch (System.NullReferenceException nre)
        {
            Debug.LogError("ScoreManager:EVENT() called whiles=null.\n" + nre);
        }
    }
    void Event(eGScoreEvent evt)
    {
        switch (evt)
        {
            //same things need to happen whether it's a draw, a win, or a loss
            //case eGScoreEvent.draw://drwaing a card
            case eGScoreEvent.gameWin:
                //won the round
            case eGScoreEvent.gameLoss://lost the round
                //chain = 0;//resets the score chain
                //score += scoreRun;//resets the score chain
                //scoreRun = 0;//reset scoreRun
                break;
            //case eGScoreEvent.mine://remove a mine card
               //chain++;//increase the score chain
                //scoreRun += chain;//add score for this card to run
                //break;
        }
        //this second switch statement handles round wins and losses
        switch (evt)
        {
            case eGScoreEvent.gameWin:
                //if it's a win, add the score to the next round
                //static fields are not reset by SceneManager.LoadScene()
                //SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                break;
            case eGScoreEvent.gameLoss:
                //if it's a loss, check against the high score
                if (HIGH_SCORE >= score)
                {
                    print("You got the high score!  High score: " + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("GolfSolitaireHighScore", score);
                }
                else
                {
                    print("Your final score for the game was: " + score);
                }
                break;
            default:
                print("score: " + score);
                break;
        }
    }
    //static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    //static public int SCORE_RUN { get { return S.scoreRun; } }
}
