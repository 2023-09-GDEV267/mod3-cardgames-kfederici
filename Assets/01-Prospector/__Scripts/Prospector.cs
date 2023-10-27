using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class Prospector : MonoBehaviour {

	static public Prospector 	S;

	[Header("Set in Inspector")]
	public TextAsset			deckXML;
	public TextAsset layoutXML;

	[Header("Set Dynamically")]
	public Deck					deck;
	public Layout layout;
	public List<CardProspector> drawPile;

	void Awake(){
		S = this;
	}

	void Start() {
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle(ref deck.cards);

		//Card c;

		//for (int cNum=0; cNum<deck.cards.Count; cNum++)
		//{
		//	c = deck.cards[cNum];
		//	c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
		//}

		layout = GetComponent<Layout> (); //get the layout component
		layout.ReadLayout(layoutXML.text); //Pass LayoutXML to it
		drawPile = ConvertListCardsToListCardProspectors(deck.cards);
	}
	List<CardProspector> ConvertListCardsToListCardProspectors(List<Card>1CD)
	{
		List<CardProspector> 1CP = new List<CardProspector> ();
		CardProspector tCP;
		foreach(Card tCD in 1CD)
		{
			tCP = tCD as CardProspector;
			1CP.Add(tCP);
		}
		return(1CP);
	}
}
