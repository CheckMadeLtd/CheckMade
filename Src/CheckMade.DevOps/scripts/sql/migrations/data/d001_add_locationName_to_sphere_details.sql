-- applied on 12/06/2025 to dev and 

UPDATE spheres_of_action
SET details = details || jsonb_build_object('LocationName', null)
WHERE NOT (details ? 'LocationName');
    
