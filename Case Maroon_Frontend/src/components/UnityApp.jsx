// src/components/UnityApp.jsx
import React, { useState, useEffect } from "react";
import { Unity, useUnityContext } from "react-unity-webgl";

function UnityApp({ onGridClick }) {
  const { unityProvider } = useUnityContext({
    loaderUrl: "/Unity/Builds/Build/Builds.loader.js",
    dataUrl: "/Unity/Builds/Build/Builds.data",
    frameworkUrl: "/Unity/Builds/Build/Builds.framework.js",
    codeUrl: "/Unity/Builds/Build/Builds.wasm",
  });
  

   useEffect(() => {
    window.SendGridPositionToJS = (json) => {
      try {
        const pos = JSON.parse(json);
        if (onGridClick) {
          onGridClick(pos);
        }
      } catch (e) {
        console.error("Failed to parse grid position from Unity:", e);
      }
    };

    return () => {
      delete window.SendGridPositionToJS;
    };
  }, [onGridClick]);
  
  const [devicePixelRatio, setDevicePixelRatio] = useState(window.devicePixelRatio);

  useEffect(() => {
    const updateDevicePixelRatio = () => {
      setDevicePixelRatio(window.devicePixelRatio);
    };

    const mediaMatcher = window.matchMedia(
      `screen and (resolution: ${devicePixelRatio}dppx)`
    );

    mediaMatcher.addEventListener("change", updateDevicePixelRatio);
    return () => {
      mediaMatcher.removeEventListener("change", updateDevicePixelRatio);
    };
  }, [devicePixelRatio]);

  return (
    <Unity
      unityProvider={unityProvider}
      style={{ width: "70%", height: "80%" }}
      devicePixelRatio={devicePixelRatio}
    />
  );
}

export default UnityApp;
