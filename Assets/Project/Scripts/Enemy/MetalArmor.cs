using UnityEngine;

/// <summary>
/// メタル敵の装甲。単一責任=「クリティカル(最大ため×先端)以外のダメージを弾く」。
/// 実際のダメージ適用はSwordHitbox。ここは『弾くか』だけ答える。
/// </summary>
public class MetalArmor : MonoBehaviour
{
    /// <summary>このヒットを弾く(=ダメージ無効)か。クリティカル以外は弾く。</summary>
    public bool ShouldBlock(bool isCrit)
    {
        return !isCrit;
    }
}