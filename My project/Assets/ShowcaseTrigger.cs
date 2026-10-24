using UnityEngine;

// Turns on the given scripts the first time the player walks into this trigger,
// e.g. to wake up the follower saw only when the player reaches its showcase station
public class ShowcaseTrigger : MonoBehaviour
{
    public Behaviour[] enableOnEnter;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            foreach (Behaviour b in enableOnEnter)
            {
                if (b != null)
                    b.enabled = true;
            }
        }
    }
}
