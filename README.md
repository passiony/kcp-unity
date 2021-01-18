# kcp-unity

KCP是一种基于UDP的上层协议，项目地址：[KCP – A Fast and Reliable ARQ Protocol](https://github.com/skywind3000/kcp)

kcp就是在udp上再做了一层封装，来实现TCP的效果并且弥补TCP的一些不足。使用kcp的方式就是和服务器连接一个udp连接，然后再udp的通信过程中，使用kcp库对字节进行处理。然后通过kcp拿到数据包。kcp可以保证你拿到的包，一，不会丢包，二，保证数据包的发送顺序和接收顺序一致。这其实就是tcp的功能。但是kcp的速度比tcp要快的多。

## KCP CSharp

unity接入kcp的话，肯定要c#版本的，但是同样是c#版本kcp，也是有区别的，

### 第一种方式：c#移植版

移植版，就是使用c#语言翻译原版c的kcp库。一般这样实现的库有很多，但是质量参差不齐，有的是完美移植，有的可能会遗留一些问题，比如gc太多等

目前用的还比较多的有：

- [kcp-csharp](https://github.com/limpo1989/kcp-csharp): kcp的 csharp移植，同时包含一份回话管理，可以连接上面kcp-go的服务端，且和kcp-go的语法相似。
- [kcp-csharp](https://github.com/KumoKyaku/KCP): 新版本 Kcp的 csharp移植。线程安全，运行时无alloc，对gc无压力。

### 第二种方式：动态库版

动态库版，就是把原版c的kcp库，进行动态库编译，编译为ios，android，pc，linux等平台的库，然后我们使用C#去封装一个udp，然后调用kcp的动态链接库。

这种方式，好处是可以保留原版库的所有特性，不用考虑优化语法的问题了，而且性能比较好。但是坏处就是编译动态库较麻烦，需要针对所有平台...可想而知

[unity3d构建kcp跨平台动态链接库](https://github.com/smilehao/kcp_bulild)

因此本仓库就是基于以上动态链接库实现的，基于[ET框架](https://github.com/egametang/ET)修改而成。
ET框架中网络有自己的封装，包括了server和client。理解起来稍微有点麻烦，所以我把部分逻辑简化，封装了 一个纯client的。其中tcp和kcp均可直接和服务器(go/java)对接

详情链接：https://passion.blog.csdn.net/article/details/112722271

# 友情链接

[skywind3000/kcp](https://github.com/skywind3000/kcp)

[xtaci/kcp-go](https://github.com/xtaci/kcp-go)
