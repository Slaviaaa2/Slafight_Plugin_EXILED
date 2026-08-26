using UnityEngine;

namespace Slafight_Plugin_EXILED.API.Features;

/// <summary>
/// Schematic をプレイヤーに追従させる MonoBehaviour。
/// SetParent の代わりにこれをアタッチすることでラグなし同期を実現。
/// </summary>
public class WearFollower : MonoBehaviour
{
    private Transform _target;
    private Vector3 _offset;
    private Quaternion _rotationOffset = Quaternion.identity;

    private Vector3 _lastPosition;
    private Quaternion _lastRotation;
    private bool _hasLastTransform;

    public void Initialize(Transform target, Vector3 offset = default, Quaternion? rotationOffset = null)
    {
        _target = target;
        _offset = offset;
        _rotationOffset = rotationOffset ?? Quaternion.identity;
        _hasLastTransform = false;
    }

    private void Update()
    {
        if (_target == null)
        {
            Destroy(this);
            return;
        }

        // オフセットをターゲットのローカル座標系で計算
        Vector3 position = _target.position + _target.TransformDirection(_offset);
        Quaternion rotation = _target.rotation * _rotationOffset;

        // 追従先が動いていないフレームでは書かない。
        // スキマティック配下のネットワークオブジェクトへ毎フレーム transform を書くと
        // 装備数 x プレイヤー数だけ同期が走る。
        if (_hasLastTransform && position == _lastPosition && rotation == _lastRotation)
            return;

        _hasLastTransform = true;
        _lastPosition = position;
        _lastRotation = rotation;

        transform.position = position;
        transform.rotation = rotation;
    }
}
