# LBTween
Simple tweener for Unity using Jobs

I created this tweener because I needed to tween rigidbodies and the libraries I found seemed to be specifically for tweening transforms. 
This library is very lightweight and is easy to hook into any system as you simply register a callback for when the tween is updated.

# Use
- Create an instance of TweenRunner and hook that into an Update call. There is an example of a Singleton MonoBehaviour running a TweenRunner in Update and another in FixedUpdate.
- Start / Stop tweens by calling the relevant function in the TweenRunner. There are a couple of example components so you can see how this is done.
- Add new GetValue functions to support tweening any values.
