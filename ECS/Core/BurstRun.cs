﻿#define BURST_BLITTABLE_CHECK

namespace ME.ECS {

    using Unity.Burst;
    using Unity.Collections.LowLevel.Unsafe;
    
    unsafe delegate void FunctionPointerDelegate(ref void* data);

    public interface IBurst {

        void Execute();

    }

    [BurstCompile(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
    public static unsafe class Burst<T> where T : struct, IBurst {

        private static FunctionPointer<FunctionPointerDelegate> cache;
        private static FunctionPointerDelegate cacheDelegate;
        
        [BurstCompile(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
        private static void Method(ref void* data) {

            UnsafeUtility.CopyPtrToStructure(data, out T j);
            j.Execute();
            UnsafeUtility.CopyStructureToPtr(ref j, data);

        }

        public static void Run(ref T data) {

            #if BURST_BLITTABLE_CHECK
            if (UnsafeUtility.IsBlittable<T>() == false) {
                
                throw new System.Exception("T must be blittable");
                
            }
            #endif

            if (Burst<T>.cache.IsCreated == false) {
                
                Burst<T>.cache = BurstCompiler.CompileFunctionPointer((FunctionPointerDelegate)Burst<T>.Method);
                Burst<T>.cacheDelegate = Burst<T>.cache.Invoke;

            }
            var objAddr = UnsafeUtility.AddressOf(ref System.Runtime.CompilerServices.Unsafe.AsRef(data));
            Burst<T>.cacheDelegate.Invoke(ref objAddr);

        }

    }

}