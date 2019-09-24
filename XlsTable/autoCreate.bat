@echo =========Compilation  All xls=========
@echo off

::---------------------------------------------------
::第一步，将xls经过xls_deploy_tool转成bin和proto
::---------------------------------------------------

::设置表的Proto所存的路径
set STEP1_XLS2PROTO_PATH=Proto

::删除文件夹中之前生成的文件
@echo off
echo TRY TO DELETE TEMP FILES:
del *_pb2.py
del *_pb2.pyc
del *.proto
del *.data
del *.log
del *.txt

for /R %worktable% %%j in (*.xls) do (
    python xls_deploy_tool.py  %%~nj  .\worktable\%%~nj.xls
)

::---------------------------------------------------
::第二步：把proto翻译成cs
::---------------------------------------------------

::设置临时存放cs文件的路径
set STEP2_PROTO2CS_PATH=.\proto2cs
::中间件的后缀名，用于生成CS文件
set PROTO_DESC=proto.protodesc
set SRC_OUT=.\


for  %%j in (*.proto) do (
echo  protoc %%~nj.proto --descriptor_set_out=%%~nj.protodesc
protoc %%~nj.proto --descriptor_set_out=%%~nj.protodesc
ProtoGen\protogen -i:%%~nj.protodesc -o:%%~nj.cs
)

::---------------------------------------------------
::第三步：将bin和cs拷到Assets里
::---------------------------------------------------

@echo off
set OUT_PATH=..\xls2ProtoDataReader\Assets
set DATA_DEST=StreamingAssets\DataConfig
set Proto_PATH = ..\Proto
set CS_DEST=CSTable

@echo on
copy *.data %OUT_PATH%\%DATA_DEST%
copy *.cs %OUT_PATH%\%CS_DEST%
copy *.proto %Proto_PATH %

::---------------------------------------------------
::第四步：清除中间文件
::---------------------------------------------------
@echo off
echo TRY TO DELETE TEMP FILES:
cd %STEP1_XLS2PROTO_PATH%
del *_pb2.py
del *_pb2.pyc
del *.log
del *.txt
del *.protodesc
del *.proto
del *.cs
del *.data

echo  autoCreateSuccess
@pause