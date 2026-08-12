using System;
using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "LevelReward", menuName = "EasyIdleGame/x_Obsolete/ LevelReward", order = 99)]
    [Obsolete(
        "LevelReward is deprecated, please use Reward instead. " +
        "This class is kept only for the backward compatibility"
        )]
    public class LevelReward : Reward { }
}