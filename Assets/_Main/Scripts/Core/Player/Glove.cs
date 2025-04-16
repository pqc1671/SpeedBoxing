using System;
using UnityEngine;

public class Glove : Node
{
    public BallColor gloveColor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            if (other.TryGetComponent(out Ball ball))
            {
                //  + Thật: Red, Yellow
                //  + Giả: White, Brown, Orange
                
                var isRealBall= ball.ballColor is BallColor.Red or BallColor.Yellow;
                Transform vfxBallExplosion= null;
                if (isRealBall)
                {
                    // Nếu là bóng thật, kiểm tra màu
                    if (ball.ballColor == gloveColor)
                    {
                        // Đấm đúng màu
                        GameManager.Instance.AddScore(10, ball.transform);
                        if (!GameManager.Instance.comboActive)
                        {
                            vfxBallExplosion= ObjectPutter.Instance.PutObject(SpawnerType.VFXRealBallExplosion);
                            vfxBallExplosion.position = ball.transform.position;
                            vfxBallExplosion.rotation = ball.transform.rotation;
                            SoundManager.Instance.PlaySound(Sound.RealBallExplosion, ball.transform.position);
                        }
                        /*Debug.Log("dung r day");*/
                    }
                    else
                    {
                        // Đấm sai màu
                        GameManager.Instance.AddScore(-20, ball.transform);
                        vfxBallExplosion= ObjectPutter.Instance.PutObject(SpawnerType.VFXFakeBallExplosion);
                        vfxBallExplosion.position = ball.transform.position;
                        vfxBallExplosion.rotation = ball.transform.rotation;
                        SoundManager.Instance.PlaySound(Sound.FakeBallExplosion, ball.transform.position);
                        /*Debug.Log("sai mau r day");*/
                    }
                }
                else
                {
                    //Bóng giả
                    GameManager.Instance.AddScore(-20, ball.transform);
                    vfxBallExplosion= ObjectPutter.Instance.PutObject(SpawnerType.VFXFakeBallExplosion);
                    vfxBallExplosion.position = ball.transform.position;
                    vfxBallExplosion.rotation = ball.transform.rotation;
                    SoundManager.Instance.PlaySound(Sound.FakeBallExplosion, ball.transform.position);
                    /*Debug.Log("bong gia r day");*/
                }
                ball.Deactivate();
            }
        }
    }
}
