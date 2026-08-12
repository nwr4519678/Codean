"use client";

import * as React from "react";
import { Play, Pause, Volume2, VolumeX, Maximize, Settings, CheckCircle2 } from "lucide-react";
import { cn } from "./utils";
import { Button } from "./button";

export interface VideoPlayerProps {
  src: string;
  poster?: string;
  title?: string;
  onComplete?: () => void;
  onProgressUpdate?: (progressPercent: number) => void;
  className?: string;
}

export function VideoPlayer({
  src,
  poster,
  title,
  onComplete,
  onProgressUpdate,
  className,
}: VideoPlayerProps) {
  const videoRef = React.useRef<HTMLVideoElement>(null);
  const containerRef = React.useRef<HTMLDivElement>(null);

  const [isPlaying, setIsPlaying] = React.useState(false);
  const [currentTime, setCurrentTime] = React.useState(0);
  const [duration, setDuration] = React.useState(0);
  const [isMuted, setIsMuted] = React.useState(false);
  const [playbackSpeed, setPlaybackSpeed] = React.useState(1);
  const [showSpeedMenu, setShowSpeedMenu] = React.useState(false);
  const [isCompleted, setIsCompleted] = React.useState(false);

  const togglePlay = () => {
    if (!videoRef.current) return;
    if (isPlaying) {
      videoRef.current.pause();
    } else {
      videoRef.current.play();
    }
    setIsPlaying(!isPlaying);
  };

  const handleTimeUpdate = () => {
    if (!videoRef.current) return;
    const curr = videoRef.current.currentTime;
    const dur = videoRef.current.duration || 1;
    setCurrentTime(curr);
    const progressPercent = Math.min(100, Math.round((curr / dur) * 100));

    if (onProgressUpdate) {
      onProgressUpdate(progressPercent);
    }

    if (progressPercent >= 90 && !isCompleted) {
      setIsCompleted(true);
      if (onComplete) onComplete();
    }
  };

  const handleLoadedMetadata = () => {
    if (videoRef.current) {
      setDuration(videoRef.current.duration);
    }
  };

  const handleSeek = (e: React.ChangeEvent<HTMLInputElement>) => {
    const time = parseFloat(e.target.value);
    setCurrentTime(time);
    if (videoRef.current) {
      videoRef.current.currentTime = time;
    }
  };

  const toggleMute = () => {
    if (!videoRef.current) return;
    videoRef.current.muted = !isMuted;
    setIsMuted(!isMuted);
  };

  const changeSpeed = (speed: number) => {
    setPlaybackSpeed(speed);
    if (videoRef.current) {
      videoRef.current.playbackRate = speed;
    }
    setShowSpeedMenu(false);
  };

  const toggleFullscreen = () => {
    if (!containerRef.current) return;
    if (!document.fullscreenElement) {
      containerRef.current.requestFullscreen().catch((err) => console.error(err));
    } else {
      document.exitFullscreen().catch((err) => console.error(err));
    }
  };

  const formatTime = (timeInSeconds: number) => {
    const minutes = Math.floor(timeInSeconds / 60);
    const seconds = Math.floor(timeInSeconds % 60);
    return `${minutes}:${seconds < 10 ? "0" : ""}${seconds}`;
  };

  return (
    <div
      ref={containerRef}
      className={cn(
        "relative group overflow-hidden rounded-xl bg-black border border-border/40 shadow-2xl transition-all",
        className
      )}
    >
      <video
        ref={videoRef}
        src={src}
        poster={poster}
        onTimeUpdate={handleTimeUpdate}
        onLoadedMetadata={handleLoadedMetadata}
        onEnded={() => {
          setIsPlaying(false);
          setIsCompleted(true);
          if (onComplete) onComplete();
        }}
        onClick={togglePlay}
        className="w-full h-auto aspect-video object-cover cursor-pointer"
      />

      {/* Title Bar Overlay */}
      {title && (
        <div className="absolute top-0 left-0 right-0 p-4 bg-gradient-to-b from-black/80 to-transparent opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-between pointer-events-none">
          <span className="text-white font-medium text-sm drop-shadow">{title}</span>
          {isCompleted && (
            <span className="flex items-center gap-1 text-xs bg-emerald-500/20 text-emerald-300 border border-emerald-500/40 px-2 py-0.5 rounded-full">
              <CheckCircle2 className="w-3.5 h-3.5" /> Completed
            </span>
          )}
        </div>
      )}

      {/* Control Bar Overlay */}
      <div className="absolute bottom-0 left-0 right-0 p-4 bg-gradient-to-t from-black/90 via-black/60 to-transparent opacity-0 group-hover:opacity-100 transition-opacity flex flex-col gap-2">
        {/* Progress Bar */}
        <input
          type="range"
          min={0}
          max={duration || 100}
          value={currentTime}
          onChange={handleSeek}
          className="w-full h-1.5 bg-white/20 rounded-lg appearance-none cursor-pointer accent-primary hover:h-2 transition-all"
        />

        <div className="flex items-center justify-between text-white text-xs pt-1">
          <div className="flex items-center gap-3">
            <Button
              type="button"
              variant="ghost"
              size="icon"
              onClick={togglePlay}
              className="w-8 h-8 text-white hover:bg-white/20 rounded-full"
            >
              {isPlaying ? <Pause className="w-4 h-4" /> : <Play className="w-4 h-4 fill-white" />}
            </Button>

            <span className="font-mono text-zinc-300">
              {formatTime(currentTime)} / {formatTime(duration)}
            </span>
          </div>

          <div className="flex items-center gap-2">
            {/* Speed Control */}
            <div className="relative">
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => setShowSpeedMenu(!showSpeedMenu)}
                className="h-8 px-2 text-white hover:bg-white/20 font-mono text-xs"
              >
                <Settings className="w-3.5 h-3.5 me-1" />
                {playbackSpeed}x
              </Button>

              {showSpeedMenu && (
                <div className="absolute bottom-10 right-0 bg-slate-900/95 border border-slate-700/80 rounded-lg p-1 shadow-xl flex flex-col gap-1 z-20">
                  {[0.75, 1, 1.25, 1.5, 2].map((speed) => (
                    <button
                      key={speed}
                      onClick={() => changeSpeed(speed)}
                      className={cn(
                        "px-3 py-1 text-left text-xs rounded hover:bg-primary/30 text-white transition-colors",
                        playbackSpeed === speed && "bg-primary text-white font-bold"
                      )}
                    >
                      {speed}x
                    </button>
                  ))}
                </div>
              )}
            </div>

            {/* Mute Toggle */}
            <Button
              type="button"
              variant="ghost"
              size="icon"
              onClick={toggleMute}
              className="w-8 h-8 text-white hover:bg-white/20 rounded-full"
            >
              {isMuted ? <VolumeX className="w-4 h-4 text-red-400" /> : <Volume2 className="w-4 h-4" />}
            </Button>

            {/* Fullscreen */}
            <Button
              type="button"
              variant="ghost"
              size="icon"
              onClick={toggleFullscreen}
              className="w-8 h-8 text-white hover:bg-white/20 rounded-full"
            >
              <Maximize className="w-4 h-4" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
