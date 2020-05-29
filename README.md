# ScrollSystem
a loop and multiple type of items scrollrect solution for unity

use unity 2019.2.6 or above to open the project

any suggestions are welcome

contact me with

email:719252885@qq.com

wechat:bandingyue



# ScrollSystem

## 特色
---
* 控件元素循环使用
* 支持多类型物体
* 多种排列方式以及滚动方式
* 支持跳转和原生进度条

## 使用说明
---
>1、Unity相关

（1）在对应的UI层级下，创建一个空的子物体
（2）把ScrollSystem.cs拖拽到空物体上，拖拽完成之后会自动创建所需的其他控件，ScrollRect，Image，mask			等，并且会创建一个名字为Content Transform的子物体作为ScrollRect的Content


![微信截图_20200529144147](https://github.com/banearth/ScrollSystem/blob/master/Image/微信截图_20200529144147.png)

![微信截图_20200529144216](Image\微信截图_20200529144216.png)

（3）调整ScrollSystem所在节点的宽高至理想大小，在content Transform节点下创建好所需要的界面元素，可		以一个，也可以是多个。调整界面元素至理想大小，手动点击ScrollSystem上的”子物体手动刷新“，来预览最		终的排列效果。需要注意的是，最后留在这个Content Transform节点下的子物体，相同物体只留下作为原始		模板

![微信截图_20200529145447](Image\微信截图_20200529145447.png)

![微信截图_20200529145817](Image\微信截图_20200529145817.png)

（4）元素排列顺序取决于ScrollSystem上面的StartCorner和ScrollDirection两个参数，基础规则为：如果当前行		或者列，能容纳下一个元素，那么会紧跟上一个元素，否则会另起新行或者列进行排列。另外，也可以根据		需要，在元素上添加ScrollLayout.cs来决定是否主动另起新行，并且可以设置在新行居中，左对齐（上对		齐），右对齐（下对齐）

![微信截图_20200529150641](Image\微信截图_20200529150641.png)

（5）至此，在Unity里的基础设置就告一段落了。关于ScrollSystem上面的一些参数的具体含义，以及ScrollLayout的一些其他用法，后面会继续介绍，这里只专注于满足基本流程的流程





---
>2、Lua相关

（1）在Main.lua中进行注册（如果满足命名规则可省略），RegistSubView后面第一个参数表示的是Content Transform下面元素的名字，第二个参数表示这个元素关联的Lua脚本
```lua
 IUIViewProvider:RegisterSubView('ServerListBaseItem', ServerListBaseItemComponent)
```

（2）如果元素的GameObject的名字为xxx，元素对应的luaComponent的名字是xxxComponent，那么第一步可以省略，在系统中会自动完成这一步，不需要手动关联

（3）元素对应的LuaComponent的标准模板如下
```lua
local M = class("ChatMessageComponent", UIScrollComponent)

function M:OnOpen()
    self.txt_Label = self:GetText('Label')
    self.bt_Check = self:GetButton('Check')
    self.bt_Check:OnClick(function() 
        --可以调用GetHandle来获知自身的Handle
    	sekf:Notify('Check', self:GetHandle())    
    end)
    self.scrollLayout = self:GetView():GetComponent('ScrollLayout')
end

function M:OnClose()
    self.txt_Label = nil
    self.bt_Check = nil
    self.scrollLayout = nil
end

function M:OnRefresh(data)
    self.txt_Label:SetTextValue(data.msg)
end

function M:OnGetSize(data)
    return 0,self.scrollLayout:GetHeightByStr(data.msg)
end

ChatMessageComponent = M
return M
```
（4）所有循环滚动的Component继承自UIScrollComponent，OnOpen里面写具体Component的获取，OnClose里面设置为空，OnRefresh里面写具体的界面元素的读取，OnGetSize表示动态设置Component的宽高

（5）UICtrl层具体的使用模板如下

```lua
--增
local handle = self.ss_ChatList:Add('ChatMessage',{msg='我是第一段聊天'})
--改
self.ss_ChatList:Set(handle,{msg='要修改的文字'})
--插
local handle2 = self.ss_ChatList:Insert(handle,{msg='我是被插入的聊天'})
--删
self.ss_ChatList:Remove(handle2)
--跳
self.ss_ChatList:ScrollTo(handle,true)
self.ss_ChatList:ScrollToBegin(true)
self.ss_ChatList:ScrollToEnd(true)
--查
local data = self.ss_ChatList:Get(handle)
print(data.msg)
--'我是第一段聊天'
--清
self.ss_ChatList:Clear()
```

（6）至此，lua层的操作都覆盖了



## 其他一些内容
---
>1、ScrollSystem上具体参数说明

![微信截图_20200529144147](Image\微信截图_20200529144147.png)

（1）centerred：元素默认会居中排列

（2）border：元素距离外边框的距离

（3）spacing：元素和元素之间的间隔

（4）resetNormalizedPosition：在初次Add或者Clear之后ScrollSystem会调整到的位置

（5）JumpToSpeed：跳转的速度

（6）registPoolCount：每个元素初次注册的对象池数量

（7）enableScrollOnlyWhenOutOfBounds：只有在元素内容超出外边框的时候才会有滚动功能

（8）scrollDirection：滚动方向，垂直或者水平

---
>2、如何最快速对接比如聊天这种动态高度的元素

（1）这里提供了两个ScrollLayout的子集，分别对应Text文字和TextMeshPro文字。把需要匹配的文字拖入FitLabel里面

![微信截图_20200529161146](Image\微信截图_20200529161146.png)

![微信截图_20200529161536](Image\微信截图_20200529161536.png)



（2）确保你的luaComponent里面写入了如下内容
```lua
function OnOpen()
	self.scrollLayout = self:GetView():GetComponent('ScrollLayout')
end

function M:OnGetSize(data)
    return 0,self.scrollLayout:GetHeightByStr(data.msg)
end
```
