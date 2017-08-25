# AnimatorControl
更加方便的对Animator的事件系统进行操控

如何使用
==
- 使用*表示将事件应用到所有的动画当中
- 使用如Attack_*表示所有带有Attack_前缀的动画名称添加到动画事件当中
- 当不带*时表示全字符匹配

例子
==
AnimatorControl.SetEvent(AnimatorEventType.Running, (animator, info, index) =>
{
  Debug.Log("Hello World")
},"Attack_*","JumpDown","*_Casting")
