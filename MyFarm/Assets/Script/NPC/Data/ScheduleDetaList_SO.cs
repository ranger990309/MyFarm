using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScheduleDetaList_SO", menuName = "NPC Schedule/ScheduleDetaList")]
public class ScheduleDetaList_SO : ScriptableObject {
    public List<ScheduleDetails> scheduleList;
}
