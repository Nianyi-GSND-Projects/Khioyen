using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LongLiveKhioyen
{
    public enum BattalionType
    {
        Melee,
        Ranged,
        NonBattle
    }
    [CreateAssetMenu(menuName = "Long Live Khioyen/Battalion Definition")]
    
    public class BattalionDefinition : ScriptableObject
    {
        public string battalionId;
        public string[] tags;
        public Sprite figure;

        public GameObject ModelTemplate => Resources.Load<GameObject>($"Models/Battalions/{battalionId}");

        public int defaultDiscipline;
        public int defaultAttack;
        public int defaultDefence;
        public int defaultFlexibility;
        public int defaultStrategy;
        
        public int defaultMaxSolider;
        public int defaultMaxMorale;
        
        public BattalionType battalionType;
        public int attackRange;

        public bool special;
    }
}