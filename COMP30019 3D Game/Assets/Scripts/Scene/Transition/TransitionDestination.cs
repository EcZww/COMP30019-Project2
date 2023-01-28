using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionDestination : MonoBehaviour
{
    public enum DestinationTag {
        ENTER, A, B, C, OnTrunk, UnderTrunk, HighPoint, LowPoint
    }

    public DestinationTag destinationTag;
   

    // public IEnumerator GetEnumerator() {
    //     IEnumerable<DestinationTag> lst = (IEnumerable<DestinationTag>) Enum.GetValues(typeof(DestinationTag));
    //     return (IEnumerator) lst;
 
    // }

}

