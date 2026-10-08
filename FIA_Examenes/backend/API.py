from fastapi import Request
from fastapi import APIRouter, UploadFile, File, HTTPException
from fastapi.responses import StreamingResponse, Response
from modules.videoCapture import videoPost

router = APIRouter(tags=["API's"])

@router.get("/video/")
async def video(request: Request):
    return videoPost(request)