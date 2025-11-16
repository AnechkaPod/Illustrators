-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create schemas (only 3 now!)
CREATE SCHEMA IF NOT EXISTS auth;
CREATE SCHEMA IF NOT EXISTS illustrator;
CREATE SCHEMA IF NOT EXISTS image;

-- Grant permissions
GRANT ALL ON SCHEMA auth TO illustrators_user;
GRANT ALL ON SCHEMA illustrator TO illustrators_user;
GRANT ALL ON SCHEMA image TO illustrators_user;

-- Auth service tables
CREATE TABLE auth.users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(500) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'User',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_email ON auth.users(email);
CREATE INDEX idx_users_role ON auth.users(role);

-- Illustrator service tables (with contact info!)
CREATE TABLE illustrator.illustrators (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    bio TEXT,
    email VARCHAR(255),
    phone VARCHAR(50),
    website_url VARCHAR(500),
    social_links JSONB,
    specialties TEXT[],
    location VARCHAR(255),
    is_featured BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE
);

CREATE INDEX idx_illustrators_user_id ON illustrator.illustrators(user_id);
CREATE INDEX idx_illustrators_featured ON illustrator.illustrators(is_featured);
CREATE INDEX idx_illustrators_specialties ON illustrator.illustrators USING GIN(specialties);
CREATE INDEX idx_illustrators_location ON illustrator.illustrators(location);

-- Image service tables
CREATE TABLE image.images (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    illustrator_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    s3_key VARCHAR(500) NOT NULL,
    s3_url VARCHAR(1000) NOT NULL,
    thumbnail_url VARCHAR(1000),
    file_size_bytes BIGINT,
    width INTEGER,
    height INTEGER,
    tags TEXT[],
    is_featured BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (illustrator_id) REFERENCES illustrator.illustrators(id) ON DELETE CASCADE
);

CREATE INDEX idx_images_illustrator_id ON image.images(illustrator_id);
CREATE INDEX idx_images_featured ON image.images(is_featured);
CREATE INDEX idx_images_tags ON image.images USING GIN(tags);

-- Trigger for updated_at columns
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON auth.users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_illustrators_updated_at BEFORE UPDATE ON illustrator.illustrators
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_images_updated_at BEFORE UPDATE ON image.images
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();