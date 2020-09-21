# EmojiText

## 介绍

在阅读UGUI源代码过程中，基于Text写的控件，目前支持图文混排，超链接的显示与点击。

使用了SpriteAtlas，支持Unity2019+的版本。

## 设计思路

* Sprite的显示

  在Text中使用特殊格式的字符串标记，解析特殊的字符串获取到Sprite的位置以及使用的SpriteName和size，获取到信息之后将其替换成quad富文本来进行占位，这样在实际显示text中就有一个空的位置，然后使用子节点的CanvasRender来讲指定信息的sprite渲染上去。

* 超链接的显示与点击

  同样的使用特殊格式的字符串标记，解析得到了所需要显示的文本内容、位置信息以及事件id，在得到信息之后直接将其替换成要显示的文本内容，然后使用子节点的CanvasRender来在对应位置画指定高度的线。点击的话即将目标字符所在的rect存储起来，然后再emojiText的点击事件中检测是否点击到对应的rect。

这里需要注意的是text里面拿到的顶点顺序是这样的

**0 1**

**3 2**

这里要感谢https://blog.csdn.net/qq992817263/article/details/51112304提供的思路。

## 坑点

Unity2019.1.5f1之后的版本里UItext不再存储完整的顶点信息，查到的信息如下

https://blog.csdn.net/wanzi215/article/details/103970269

导致本来是正常计算的位置信息需要做一些处理..这个是否包含富文本之类的顶点信息也比较邪门，不止是新版本text就不包含这么简单。研究之后发现和是否自动换行了有关(示例工程项目为Unity2019.3.1f1)，因此项目中有如下函数

```c#
private bool CheckIsNewCalculateWay()
{
    Vector2 extents = rectTransform.rect.size;

    var settings = GetGenerationSettings(extents);
    var actuallyText = _curentTagParser.ActuallyText;
    cachedTextGenerator.PopulateWithErrors(actuallyText, settings, gameObject);
    
    var allTextLineCount = Regex.Matches(_curentTagParser.ActuallyText, @"[\r\n]").Count + 1;
    return (allTextLineCount == cachedTextGenerator.lineCount);
}
```

如果有什么新的判别方法都可以告诉我。

## 效果展示

![img](https://github.com/jechyang/EmojiText/blob/master/ReadMeImage/emojitext.gif)

## 使用说明

### Component挂载相关

* 将要使用的sprite打成图集，然后给GameObject挂载EmojiText控件。
* 给EmojiText添加子节点，并挂载EmojiTextSprie控件用来绘制Sprite，并且将图集赋值。这里除了图集以外还需要一个值:**AnySpriteName**，主要是只持有Atlas没办法获取到对应的texture，所以需要拿到任意一张sprite，然后使用sprite.texture拿到对应的texture(如果有其他方法获取的话可以提issue给我)
* 给EmojiText添加子节点，并挂载EmojiTextHref控件来绘制超链接

最后节点状态应该如下图，注意一定是要两个节点的，需要两个canvasRender绘制不同的内容:

![img](https://github.com/jechyang/EmojiText/blob/master/ReadMeImage/node.png)

EmojiTextSprite和EmojiTextHref，都是需要功能的时候才需要挂载，例如不需要超链接功能就不需要第二个子节点

### 输入格式相关

这里标签的统一格式为**<#[A-Z] (params)>**

* Emoji的格式为 :**<#E size name>**

  例如Emoji表情的话就是<#E 30 1> 注意这里的空格，是代表分隔符，后面的30代表的是emoji的size,后面的1即为spirte的name

* 超链接的输入格式为**<#H content eventId>**

  content即为要显示的内容，eventId是可选的，如果有的话就需要在代码中绑定对应的事件，注册事件代码如下

```c#
_emojiText = GetComponent<EmojiText>();
_emojiText.AddClickListener(1, () =>
{
    Debug.LogError("hello world");
});
```

这里各个参数之间的(包括#[A-Z]和params之间的)分隔符暂定为空格，如果有需要的话可以通过更改**Consts.TagSplitChar**的值来进行更改。