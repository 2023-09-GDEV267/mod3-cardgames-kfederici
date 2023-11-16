using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class Golf : MonoBehaviour
{

    static public Golf S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    public float reloadDelay = 2f;//2 second delay between rounds
    public Text gameOverText, roundResultText, highScoreText;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardGolfSolitaire> drawPile;
    public Transform layoutAnchor;
    public CardGolfSolitaire target;
    public List<CardGolfSolitaire> tableau;
    public List<CardGolfSolitaire> discardPile;
    public FloatingScore fsRun;

    void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        //set up the HighScore UI Text
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        go.GetComponent<Text>().text = hScore;

        //set up the UI Texts that show at the end of the round
        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }
        //make the end of round texts invisible
        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;
        deck = GetComponent<Deck>();//get the deck
        deck.InitDeck(deckXML.text);//pass DeckXML to it
        Deck.Shuffle(ref deck.cards);//this shuffles the deck by reference

        //Card c;

        //for (int cNum=0; cNum<deck.cards.Count; cNum++)
        //{
        //	c = deck.cards[cNum];
        //	c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        //}

        layout = GetComponent<Layout>(); //get the layout component
        layout.ReadLayout(layoutXML.text); //Pass LayoutXML to it
        drawPile = ConvertListCardsToListCardGolfSolitaire(deck.cards);
        LayoutGame();
    }

    List<CardGolfSolitaire> ConvertListCardsToListCardGolfSolitaire(List<Card> lCD)
    {
        List<CardGolfSolitaire> lCP = new List<CardGolfSolitaire>();
        CardGolfSolitaire tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardGolfSolitaire;
            lCP.Add(tCP);
        }
        return (lCP);
    }

    //the draw function will pull a single card from the drawPile and return it
    CardGolfSolitaire Draw()
    {
        CardGolfSolitaire cd = drawPile[0]; //pull the 0th CardProspector
        drawPile.RemoveAt(0); //then remove it from List<> drawPile
        return (cd); //and return it
    }

    //LayoutGame() positions the initial tableau of cards (the mine)
    void LayoutGame()
    {
        //create an empty GameObject to serve as an anchor for the tableau
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            //create an empty GameObject named _LayoutAnchor in the hierarchy
            layoutAnchor = tGO.transform; //grab its transform
            layoutAnchor.transform.position = layoutCenter; //position it
        }
        CardGolfSolitaire cp;
        //follow the layout
        foreach (SlotDef tSD in layout.slotDefs)
        {
            //iterate through all the SlotDefs in the layout.slotDefs as tSD
            cp = Draw();//pull a card from the top of the draw pile
            cp.faceUp = tSD.faceUp;//set its faceUp to the value in SlotDef
            cp.transform.parent = layoutAnchor;//make its parent layoutAnchor
                                               //this replaces the previous parent: deck.deckAnchor, which appears as _Deck in the Hierarchy when the scene is playing
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID);
            //Set the localPosition of the card based on slotDef
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            //CardProspectors in the tableau have the state CardState.tableau
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);//set the sorting layers
            tableau.Add(cp); //Add this CardProspector to the List<> tableau
        }
        //set which cards are hiding others
        foreach (CardGolfSolitaire tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }
        //set up the initial target card
        MoveToTarget(Draw());
        //set up the Draw pile
        UpdateDrawPile();
    }
    //convert from the layoutID int to the CardProspector with that ID
    CardGolfSolitaire FindCardByLayoutID(int layoutID)
    {
        foreach (CardGolfSolitaire tCP in tableau)
        {
            //search through all cards in the tableau List<>
            if (tCP.layoutID == layoutID)
            {
                //if the card has the same ID, return it
                return (tCP);
            }
        }
        //if it's not found, return null
        return (null);
    }
    //this turns cards in the mine face up or face down
    void SetTableauFaces()
    {
        foreach (CardGolfSolitaire cd in tableau)
        {
            bool faceUp = true;//assume the card will be face up
            
            cd.faceUp = faceUp;//set the value on the card
        }
    }
    //moves the current target to the discardPile
    void MoveToDiscard(CardGolfSolitaire cd)
    {
        //set the state of the card to discard
        cd.state = eCardState.discard;
        discardPile.Add(cd);//Add it to the discardPile List<>
        cd.transform.parent = layoutAnchor;//update its transform parent
                                           //position this card on the discardPile
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.faceUp = true;
        //place it on top of the pilefor depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }
    //make cd the new target card
    void MoveToTarget(CardGolfSolitaire cd)
    {
        //if there is currently a target card, move it to discardPile
        if (target != null) MoveToDiscard(target);
        target = cd;//cd is the new target
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;
        //move to the target position
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);
        cd.faceUp = true;//make it face up
                         //set the depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }
    //arranges all the cards of the drawPile to show how many are left
    void UpdateDrawPile()
    {
        CardGolfSolitaire cd;
        //go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            //position it correctly with the layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);

        }
    }
    //CardClicked is called any time a card in the game is clicked
    public void CardClicked(CardGolfSolitaire cd)
    {
        //the reaction is determined by the state of the clicked card
        switch (cd.state)
        {
            case eCardState.target:
                //clicking the target card does nothing
                break;

            case eCardState.drawpile:
                //clicking any card in the drawPile will draw the next card
                MoveToDiscard(target);//moves the target to the discardPile
                MoveToTarget(Draw());//moves the next drawn card to the target
                UpdateDrawPile();//restacks the drawPile
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;
            case eCardState.tableau:
                //clicking a card in the tableau will check if it's a valid play
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    //if the card is face-down, it's not valid
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))
                {
                    //if it's not an adjacent rank, it' not valid
                    validMatch = false;
                }
                if (!validMatch) return;//return if not valid
                                        //if we got here, then it's a valid card
                tableau.Remove(cd);//remove it from the tableau list
                MoveToTarget(cd);//make it the target card
                SetTableauFaces();//update tableau card face ups
                ScoreManager.EVENT(eScoreEvent.mine);
                FloatingScoreHandler(eScoreEvent.mine);
                break;
        }
        CheckForGameOver();
    }
    //test whether the game is over
    void CheckForGameOver()
    {
        //if the tableau is empty, the game is over
        if (tableau.Count == 0)
        {
            //call GameOver() with a win
            GameOver(true);
            return;
        }
        //if there are still cards in the draw pile, the game's not over
        if (drawPile.Count > 0)
        {
            return;
        }
        //check for remaining valid plays
        foreach (CardGolfSolitaire cd in tableau)
        {
            if (AdjacentRank(cd, target))
            {
                //if there is a valid play, the game's not over
                return;
            }
        }
        //since there are no valid plays, the game is over
        //call GameOver with a loss
        GameOver(false);
    }
    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (fsRun != null) score += fsRun.score;
        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            //print("Game Over.  You won! :)"); commented out
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else
        {
            gameOverText.text = "Game Over";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);
            //print("Game Over.  You lost. :("); commented out
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
        }
        //reload the scene, resetting the game
        //SceneManager.LoadScene("__Prospector_Scene_0");//commented out

        //reload the scene in reloadDelay seconds
        //this will give thescore a moment to travel
        Invoke("ReloadLevel", reloadDelay);
    }
    void ReloadLevel()
    {
        //reload the scene, resetting the game
        SceneManager.LoadScene("__GolfSolitaire");
    }
    //return true if the two cards are adjacent in rank (aces and kings wrap around)
    public bool AdjacentRank(CardGolfSolitaire c0, CardGolfSolitaire c1)
    {
        //if either card is face down, it's not adjacent
        if (!c0.faceUp || !c1.faceUp) return (false);
        //if they are 1 apart, they are adjacent
        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }
        //otherwise, return false
        return (false);
    }
    //handle FloatingScore movement
    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            //same things need to happen whether it's a draw, a win, or a loss
            case eScoreEvent.draw://drawing a card
            case eScoreEvent.gameWin://won the round
            case eScoreEvent.gameLoss://lost the round
                                      //add fsRun to the Scoreboard score
                if (fsRun != null)
                {
                    //create points for the bezier curve1
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    //also adjust the fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null;//clear fsRun so it's created again
                }
                break;
            case eScoreEvent.mine://remove a mine card
                                  //create a floatingScore for this score
                FloatingScore fs;
                //move it from the mousePosition to fsPosRun
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }
}