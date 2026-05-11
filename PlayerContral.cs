using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContral : MonoBehaviour
{
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator=GetComponent<Animator>(); 
    }

    // Update is called once per frame
    void Update()
    {
        //水平轴
        float horizontal = Input.GetAxis("Horizontal");
        //垂直轴
        float vertical = Input.GetAxis("Vertical");
        //向量
        Vector3 dir=new Vector3(horizontal,0,vertical);
        if(dir!=Vector3.zero)
        {
            //面向向量
            transform.rotation = Quaternion.LookRotation(dir);
            //播放奔跑动画
            animator.SetBool("IsRun", true);
            //朝向前方移动
            transform.Translate(Vector3.forward * 2 * Time.deltaTime);
        }
        else
        {
            //播放站立动画
            animator.SetBool("IsRun", false);
        }
    }
}
