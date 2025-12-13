using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Particles;
public class ParticleManager<T>
{
    private class CircularParticleArray
    {
        private int start;
        public int Start
        {
            get { return start; }
            set { start = value % list.Length; }
        }
        public int Count { get; set; }
        public int Capacity { get { return list.Length; } }
        private Particle<T>[] list;
        public CircularParticleArray(int capacity)
        {
            list = new Particle<T>[capacity];
        }
        public Particle<T> this[int i]
        {
            get { return list[(start + i) % list.Length]; }
            set { list[(start + i) % list.Length] = value; }
        }
    }

    // This delegate will be called for each particle.
    private readonly Action<Particle<T>, float> updateParticle;
    private CircularParticleArray particleList;
    public ParticleManager(int capacity, Action<Particle<T>> updateParticle)
    {
        this.updateParticle = (p, dt) => updateParticle(p);
        particleList = new CircularParticleArray(capacity);
        // Populate the list with empty particle objects, for reuse. 
        for (int i = 0; i < capacity; i++)
            particleList[i] = new Particle<T>();
    }

    public ParticleManager(int capacity, Action<Particle<T>, float> updateParticle)
    {
        this.updateParticle = updateParticle;
        particleList = new CircularParticleArray(capacity);
        // Populate the list with empty particle objects, for reuse.
        for (int i = 0; i < capacity; i++)
            particleList[i] = new Particle<T>();
    }

    public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, Vector2 scale, T state, float theta = 0, ParticleBlendMode blendMode = ParticleBlendMode.Alpha)
    {
        Particle<T> particle;
        if (particleList.Count == particleList.Capacity)
        {
            // if the list is full, overwrite the oldest particle, and rotate the circular list 
            particle = particleList[0];
            particleList.Start++;
        }
        else
        {
            particle = particleList[particleList.Count];
            particleList.Count++;
        }
        // Create the particle 
        particle.Texture = texture;
        particle.Position = position;
        particle.Tint = tint;
        particle.Duration = duration;
        particle.PercentLife = 1f;
        particle.Scale = scale;
        particle.Orientation = theta;
        particle.State = state;
        particle.BlendMode = blendMode;
    }

    public void Update(float dtSeconds)
    {
        int removalCount = 0;
        for (int i = 0; i < particleList.Count; i++)
        {
            var particle = particleList[i];

            updateParticle(particle, dtSeconds);
            if (particle.Duration > 0f)
                particle.PercentLife -= dtSeconds / particle.Duration;
            else
                particle.PercentLife = -1f;

            // sift deleted particles to the end of the list 
            Swap(particleList, i - removalCount, i);
            // if the particle has expired, delete this particle 
            if (particle.PercentLife < 0)
                removalCount++;
        }
        particleList.Count -= removalCount;
    }

    public void Update()
    {
        Update(1f / 60f);
    }
    private static void Swap(CircularParticleArray list, int index1, int index2)
    {
        var temp = list[index1];
        list[index1] = list[index2];
        list[index2] = temp;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < particleList.Count; i++)
        {
            var particle = particleList[i];
            Vector2 origin = new Vector2(particle.Texture.Width / 2f, particle.Texture.Height / 2f);
            spriteBatch.Draw(particle.Texture, particle.Position, null, particle.Tint, particle.Orientation, origin, particle.Scale, 0, 0);
        }
    }

    public void Draw(SpriteBatch spriteBatch, ParticleBlendMode blendMode)
    {
        for (int i = 0; i < particleList.Count; i++)
        {
            var particle = particleList[i];
            if (particle.BlendMode != blendMode)
                continue;

            Vector2 origin = new Vector2(particle.Texture.Width / 2f, particle.Texture.Height / 2f);
            spriteBatch.Draw(particle.Texture, particle.Position, null, particle.Tint, particle.Orientation, origin, particle.Scale, 0, 0);
        }
    }
}

