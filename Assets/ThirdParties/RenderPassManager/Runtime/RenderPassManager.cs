using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RenderPassPipeline
{
    public class RenderPassManager
    {
        public static RenderPassManager instance = new RenderPassManager();

        public bool AppendAddPassInterfaces(
            IAddPassInterface theInterface)
        {
            if ((theInterface == null) || 
                m_addPassInterfaces.Contains(theInterface)
                || !theInterface.PlatformSupported(Application.platform))
                return false;
            if (theInterface.IsSingleton())
            {
                var type = theInterface.GetType();
                foreach (var pass in m_addPassInterfaces)
                {
                    if (pass.GetType() == type)
                        return false;
                }
            }

            theInterface.OnRegister();
            m_addPassInterfaces.Add(theInterface);
            //这里同一个RenderEvent里效果需要排序
            m_addPassInterfaces.Sort(passSort);
            return true;
        }
        public bool RemoveAddPassInterfaces(
            IAddPassInterface theInterface)
        {
            theInterface.OnDispose();
            return m_addPassInterfaces.Remove(theInterface);
        }
        
        public bool ReInitialize()
        {   
            Release();
            return true;
        }
        public bool Release()
        {   
            foreach (var pass in m_addPassInterfaces)
            {
                pass.OnDispose();
            }
            m_addPassInterfaces.Clear();
            return true;
        }
        public void ProcessAddRenderPasses(ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {   
            /*var element = m_addPassInterfaces.GetEnumerator();
            while (element.MoveNext())
                element.Current.OnAddPass(renderer, ref renderingData);
            element.Dispose();*/
            foreach (var pass in m_addPassInterfaces)
            {
                pass.OnAddPass(renderer,ref renderingData);
            }
        }

        private PassSortHelper passSort = new PassSortHelper();
        private List<IAddPassInterface> m_addPassInterfaces =
            new List<IAddPassInterface>();
    }
    
    public class PassSortHelper : IComparer<IAddPassInterface>
    {
        public int Compare(IAddPassInterface A, IAddPassInterface B)
        {
            return A.GetType() - B.GetType();
        }
    }

    public interface IAddPassInterface
    {
        /// <summary>
        /// 每帧在渲染前调用
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="renderingData"></param>
        void OnAddPass(ScriptableRenderer renderer,
            ref RenderingData renderingData);

        /// <summary>
        /// 加入渲染管线
        /// </summary>
        void OnRegister();

        /// <summary>
        /// 从管线移除
        /// </summary>
        void OnDispose();
        
        int GetType();

        /// <summary>
        /// 定义是否单例
        /// </summary>
        /// <returns></returns>
        bool IsSingleton();

        /// <summary>
        /// 判断兼容性
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        bool PlatformSupported(RuntimePlatform platform);
    };
}

