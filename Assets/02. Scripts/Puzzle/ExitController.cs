using System.Collections.Generic;
using UnityEngine;

public class ExitController : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> conditions;
    [SerializeField] private GameObject blockingObject;

    private void Start()
    {
        foreach (var mb in conditions)
        {
            if (mb is IPuzzleCondition condition)
                condition.OnConditionMet += CheckAllConditions;
        }
        CheckAllConditions();
    }

    private void CheckAllConditions()
    {
        foreach (var mb in conditions)
        {
            if (mb is IPuzzleCondition condition && !condition.IsMet)
                return;
        }
        OpenExit();
    }

    private void OpenExit()
    {
        var target = blockingObject != null ? blockingObject : gameObject;

        var anim = target.GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Open");

        var col = target.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}
