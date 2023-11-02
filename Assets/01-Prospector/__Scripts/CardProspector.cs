using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//an enum defines a variable type with a few prenamed values
public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}
public class CardProspector : Card
{
    [Header("Set Dynamically: CardProspector")]
    //this is how you use the enum eCardState
    public eCardState state = eCardState.drawpile;
    //the hiddenBy list stores which other cards will keep this one face down
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    //the layoutID matches this card to the tableau XML if it's a tableau card
    public int layoutID;
    //the slotdef class stores information pulled in from the LayoutXML <slot>
    public SlotDef slotDef;

    //this allows the card to react to being clicked
    override public void OnMouseUpAsButton()
    {
        //call the CardClicked method on the Prospector singleton
        Prospector.S.CardClicked(this);
        //also call the base class (Card.cs) version of this method
        base.OnMouseUpAsButton();
    }
}
