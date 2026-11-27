using UnityEngine;
using Animancer;

namespace CupkekGames.Animations.Animancer
{
  /// <summary>
  /// A humanoid's feet never sink below the floor she stands on. Clips made for longer legs, retargeted onto a short
  /// body, can sink a foot well into the ground (a lunge), and Unity's own foot IK keeps it there, its goals sunk too.
  /// Each foot goes to its clip's goal, never lower than her floor (the height of this transform) plus the sole, and
  /// the leg bends to reach it. The feet stay on the IK pass every frame, so a foot never jumps between where the IK
  /// and where the bones would put it as it crosses the floor.
  /// Add it where the feet stand on the floor (a fight's field); a seated pose that hangs the feet below the root on
  /// purpose goes without it.
  /// </summary>
  [RequireComponent(typeof(Animator))]
  [RequireComponent(typeof(AnimancerComponent))]
  public class FootGrounding : MonoBehaviour
  {
    private Animator _animator;

    private void Awake()
    {
      _animator = GetComponent<Animator>();
      // Unity calls OnAnimatorIK only for layers that run the IK pass.
      GetComponent<AnimancerComponent>().Layers[0].ApplyAnimatorIK = true;
    }

    private void OnAnimatorIK(int layerIndex)
    {
      if (layerIndex != 0) return;
      float floor = transform.position.y;
      Ground(AvatarIKGoal.LeftFoot, floor + _animator.leftFeetBottomHeight);
      Ground(AvatarIKGoal.RightFoot, floor + _animator.rightFeetBottomHeight);
    }

    private void Ground(AvatarIKGoal foot, float lowest)
    {
      Vector3 goal = _animator.GetIKPosition(foot);
      if (goal.y < lowest) goal.y = lowest;
      _animator.SetIKPosition(foot, goal);
      _animator.SetIKRotation(foot, _animator.GetIKRotation(foot));
      _animator.SetIKPositionWeight(foot, 1f);
      _animator.SetIKRotationWeight(foot, 1f);
    }
  }
}
